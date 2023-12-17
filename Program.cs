using YoutubeExplode;

using System.Reflection;
using System.Text;
using Azure;
using Azure.AI.OpenAI;

namespace TeleprompterConsole;

internal class Program
{
    public static string? OpenAiApiKey { get; } = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY");
    public static string? OpenAiApiEndpoint { get; } = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
    public static string? DeploymentName { get; } = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME");
    public static OpenAIClient? AzureOpenAiClient;

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


        if (string.IsNullOrEmpty(OpenAiApiKey) || string.IsNullOrEmpty(OpenAiApiEndpoint))
        {
            Console.WriteLine("Please make sure to set all your env variables: openai_api_key, AZURE_OPENAI_KEY, AZURE_OPENAI_ENDPOINT, context");
            return;
        }

        AzureOpenAiClient = new OpenAIClient(new Uri(OpenAiApiEndpoint), new AzureKeyCredential(OpenAiApiKey));

        var track = await SetUpServices(args[0]);
        await GenerateCaptions(args[0], int.Parse(args[1]), track);
    }

    private static async Task<YoutubeExplode.Videos.ClosedCaptions.ClosedCaptionTrack> SetUpServices(string videoUrl)
    {
        var youtube = new YoutubeClient();

        var trackManifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(
            videoUrl
        );
        var trackInfo = trackManifest.GetByLanguage("en");
        var track = await youtube.Videos.ClosedCaptions.GetAsync(trackInfo);

        return track;
    }

    private static async Task GenerateCaptions(string videoUrl, int slices, YoutubeExplode.Videos.ClosedCaptions.ClosedCaptionTrack track)
    {
        Console.WriteLine($"Generating {slices} timestamps for " + videoUrl);

        int captionsPerSlice = track.Captions.Count / slices;
        int startIndex = 0;
        int endIndex = captionsPerSlice;


        for (int l = 0; l < slices; l++)
        {
            var caption = track.Captions[startIndex];

            Console.Write(caption.Offset.ToString().Split('.')[0] + " ");
            var captions = GetCaptionsForSlice(track, startIndex, endIndex);
            var summary = await GetSummaryFromOpenAI(captions);


            Console.WriteLine(summary);


            if (endIndex + captionsPerSlice < track.Captions.Count)
            {
                startIndex += captionsPerSlice;
                endIndex += captionsPerSlice;
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
    private static async Task<string> GetSummaryFromOpenAI(string captions)
    {
        if (AzureOpenAiClient == null)
        {
            throw new Exception("AzureOpenAiClient is null");
        }

        var chatCompletionsOptions = new ChatCompletionsOptions()
        {
            DeploymentName = DeploymentName,
            Messages =
    {
        new ChatMessage(ChatRole.System, "You are a YouTube video summarizer. Your task is to create a summary from the captions of a youtube video. Each text I give you is only a part of an entire video transcript. I will give you a video transcript and you will summarize it in 6 words or less."),
        new ChatMessage(ChatRole.User, $" Summarize the text below, delimited by triple backticks, in at most in 6 words.\n Text: ```{captions}```")
    },
            MaxTokens = 100
        };




        Response<ChatCompletions> response = await AzureOpenAiClient.GetChatCompletionsAsync(chatCompletionsOptions);
        string completion = response.Value.Choices[0].Message.Content;

        return completion;
    }

}