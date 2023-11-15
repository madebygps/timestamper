using YoutubeExplode;

using System.Reflection;
using System.Text;

using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels;
using OpenAI;

namespace TeleprompterConsole;

internal class Program
{

    public static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            var versionString = Assembly.GetEntryAssembly()?
                                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                                    .InformationalVersion
                                    .ToString();
            Console.WriteLine($"timestamper v{versionString}");
            Console.WriteLine("-------------");
            Console.WriteLine("\nUsage:");
            Console.WriteLine("  timestamper <videoUrl> <number of timestamps>");
            return;
        }

        string? apiKey = Environment.GetEnvironmentVariable("openai_api_key");

        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("Please set the openai_api_key environment variable");
            return;
        }

        var track = await SetUpServices(args[0], Int32.Parse(args[1]));
        await GenerateCaptions(args[0], Int32.Parse(args[1]), track, apiKey!);


    }

    private static async Task<YoutubeExplode.Videos.ClosedCaptions.ClosedCaptionTrack> SetUpServices(string videoUrl, int slices)
    {
        var youtube = new YoutubeClient();

        var trackManifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(
            videoUrl
        );
        var trackInfo = trackManifest.GetByLanguage("en");
        var track = await youtube.Videos.ClosedCaptions.GetAsync(trackInfo);

        return track;
    }

    private static async Task GenerateCaptions(string videoUrl, int slices, YoutubeExplode.Videos.ClosedCaptions.ClosedCaptionTrack track, string apiKey)
    {
        Console.WriteLine($"Generating {slices} timestamps for " + videoUrl);

        int captionsPerSlice = track.Captions.Count / slices;
        int startIndex = 0;
        int endIndex = captionsPerSlice;

        var openAiService = new OpenAIService(new OpenAiOptions()
        {
            ApiKey = apiKey
        });

        for (int l = 0; l < slices; l++)
        {
            var caption = track.Captions[startIndex];

            Console.Write(caption.Offset.ToString().Split('.')[0] + " ");
            var captions = GetCaptionsForSlice(track, startIndex, endIndex);
            var summary = await GetSummaryFromOpenAI(openAiService, captions);


            Console.WriteLine(summary);


            if (endIndex + captionsPerSlice < track.Captions.Count)
            {
                startIndex = startIndex + captionsPerSlice;
                endIndex = endIndex + captionsPerSlice;
            }
            else
            {
                startIndex = endIndex;
            }
        }
    }

    private static string GetCaptionsForSlice(YoutubeExplode.Videos.ClosedCaptions.ClosedCaptionTrack track, int startIndex, int endIndex)
    {
        var captionsBuilder = new StringBuilder();
        for (int k = startIndex; k < endIndex; k++)
        {
            var caption = track.Captions[k];
            if (!string.IsNullOrWhiteSpace(caption.Text))
            {
                var captionText = caption.Text.Replace('\n', ' ');
                captionsBuilder.Append($"{captionText} ");
            }
        }
        return captionsBuilder.ToString().Trim();
    }
    private static async Task<string> GetSummaryFromOpenAI(OpenAIService openAiService, string captions)
    {


        var request = new ChatCompletionCreateRequest
        {
            Messages = new List<ChatMessage>
        {
            ChatMessage.FromSystem("You are a Youtube video editor."),
            ChatMessage.FromUser($"Your task is to create a summary from the captions of a youtube video. Each text I give you is only a part of an entire video transcript. Summarize the text below, delimited by triple backticks, in at most in 6 words.\n Text: ```{captions}```"),
        },
            Model = Models.ChatGpt3_5Turbo
        };

        // Send the request to OpenAI's API and wait for the response
        var response = await openAiService.ChatCompletion.CreateCompletion(request);


        if (response.Successful)
        {
            var summaryBuilder = new StringBuilder();
            if (response.Choices.Count > 0)
            {
                var summary = response.Choices.First().Message.Content;
                var index = summary.IndexOf("Index =");
                if (index >= 0)
                {
                    summary = summary.Substring(0, index);
                }
                summaryBuilder.Append(summary);
            }
            return summaryBuilder.ToString().Trim();
        }
        else
        {
            // Handle different error codes using a switch statement
            switch (response.Error?.Code)
            {
                case "com.openai.api.errors.TooManyRequestsError":
                    throw new Exception("Too many requests to OpenAI's API. Please wait and try again later.");
                case "com.openai.api.errors.AuthenticationFailedError":
                    throw new Exception("Authentication failed. Please check your API key and try again.");
                default:
                    throw new Exception($"OpenAI API error: {response.Error?.Message}");
            }
        }
    }

}