// See https://aka.ms/new-console-template for more information

// https://www.nuget.org/packages/YoutubeExplode

using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using YoutubeExplode;

string youtubeUrl = "https://www.youtube.com/watch?v=LgWRbXw9dRU";

var youtube = new YoutubeClient();

var openAiService = new OpenAIService(new OpenAI.GPT3.OpenAiOptions()
{
    ApiKey = ""
});

// You can specify either video ID or URL
var video = await youtube.Videos.GetAsync(youtubeUrl);

//var title = video.Title; // "Collections - Blender 2.80 Fundamentals"
//var author = video.Author.ChannelTitle; // "Blender"
var duration = video.Duration; // 00:07:20

using StreamWriter file = new("WriteLines.txt", append: true);

var trackManifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(
    youtubeUrl
);

// Find closed caption track in English
var trackInfo = trackManifest.GetByLanguage("en");

var track = await youtube.Videos.ClosedCaptions.GetAsync(trackInfo);

Console.WriteLine($"The amount of captions is {track.Captions.Count}");

int slices = Convert.ToInt32(Math.Floor(duration.Value.TotalMinutes / 5)); // 11 because int
int captionsPerSlice = track.Captions.Count / slices;

Console.WriteLine($"The amount of slices is {slices}");
Console.WriteLine($"The amount of captionSlices per slice is {captionsPerSlice}");

int startIndex = 0;
int endIndex = track.Captions.Count / slices;

for (int l = 0; l < slices; l++)
{
    var caption = track.Captions[startIndex];
    await file.WriteLineAsync($"TIMESTAMP: {caption.Offset}");
    Console.WriteLine($"Starting: {startIndex} Ending: {endIndex}");
    string words = "";

    for (int k = startIndex; k < endIndex; k++)
    {

        caption = track.Captions[k];
        if (!string.IsNullOrWhiteSpace(caption.Text))
        {
            words += $"{caption.Text} ";
            //await file.WriteLineAsync($"{caption.Text}");
        }
        // TODO: Make Open API calls for every time stamp chunk and save completion to a file. 
    }
    var completionResult = await openAiService.Completions.CreateCompletion(new CompletionCreateRequest()
    {
        Prompt = $"(Summarize the following in 10 words: {words} Summary:",
        Model = Models.TextDavinciV3,
        Temperature = (float?)0.67,
        TopP = 1,
        MaxTokens = 256,
        FrequencyPenalty = 0,
        PresencePenalty = 0

    });

    if (completionResult.Successful)
    {
        Console.WriteLine(completionResult.Choices.FirstOrDefault());
        await file.WriteLineAsync($"{completionResult.Choices.FirstOrDefault()}");
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
}

Console.WriteLine($"Last Starting: {startIndex} Ending: {endIndex}");
/*
for (var m = endIndex; m < track.Captions.Count; m++)
{
    var caption = track.Captions[m];
    // Check if the last caption is not empty
    if (!string.IsNullOrWhiteSpace(caption.Text))
    {
        await file.WriteLineAsync($"{caption.Text}");
    }

}*/

// TODO: Make Open API calls for every time stamp chunk and save completion to a file. 


