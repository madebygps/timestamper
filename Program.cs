using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using YoutubeExplode;

using System.Reflection;

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
            Console.WriteLine("  timestamper <videoUrl>");
            return;
        }
        await GenerateCaptions(args[0], Int32.Parse(args[1]));
    }

    public static async Task GenerateCaptions(string videoUrl, int slices)
    {
        Console.WriteLine($"Generating {slices} timestamps for " + videoUrl);
        var youtube = new YoutubeClient();

        var openAiService = new OpenAIService(new OpenAI.GPT3.OpenAiOptions()
        {
            ApiKey = "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"
        });

        var trackManifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(
            videoUrl
        );

        // Find closed caption track in English
        var trackInfo = trackManifest.GetByLanguage("en");

        var track = await youtube.Videos.ClosedCaptions.GetAsync(trackInfo);

        //int slices = 20;
        int captionsPerSlice = track.Captions.Count / slices;

        int startIndex = 0;
        int endIndex = captionsPerSlice;
        string words = "";

        for (int l = 0; l < slices; l++)
        {
            var caption = track.Captions[startIndex];

            Console.Write(caption.Offset.ToString().Split('.')[0] + ": ");
            words = "";

            for (int k = startIndex; k < endIndex; k++)
            {
                caption = track.Captions[k];
                if (!string.IsNullOrWhiteSpace(caption.Text))
                {
                    words += $"{caption.Text}";
                }
            }
            var completionResult = await openAiService.Completions.CreateCompletion(new CompletionCreateRequest()
            {
                Prompt = $"(Summarize the following in 5 words: {words} Summary:",
                Model = Models.TextDavinciV3,
                Temperature = (float?)0.67,
                TopP = 1,
                MaxTokens = 256,
                FrequencyPenalty = 0,
                PresencePenalty = 0
            });

            if (completionResult.Successful)
            {
                string summary = completionResult.Choices.FirstOrDefault().ToString().Remove(0, 25);
                int index = summary.IndexOf("Index =");
                if (index >= 0)
                {
                    summary = summary.Substring(0, index);
                }
                Console.WriteLine(summary);
            }
            else
            {
                if (completionResult.Error == null)
                {
                    throw new Exception("Unknown Error");
                }
                Console.WriteLine($"{completionResult.Error.Code}: {completionResult.Error.Message}");
            }

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
}

// You can specify either video ID or URL
