// See https://aka.ms/new-console-template for more information

// https://www.nuget.org/packages/YoutubeExplode

using YoutubeExplode;

string youtubeUrl = "https://www.youtube.com/watch?v=LgWRbXw9dRU";

var youtube = new YoutubeClient();

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

    for (int k = startIndex; k < endIndex; k++)
    {
        caption = track.Captions[k];
        await file.WriteLineAsync($"{caption.Text}");
    }
    if (endIndex + captionsPerSlice < track.Captions.Count)
    {
        startIndex = startIndex + captionsPerSlice;
        endIndex = endIndex + captionsPerSlice;
    }
}

Console.WriteLine($"Last Starting: {startIndex} Ending: {endIndex}");

for (var m = endIndex; m < track.Captions.Count; m++)
{
    var caption = track.Captions[m];
    await file.WriteLineAsync($"{caption.Text}");
}

/*for (int i = 0; i < track.Captions.Count; i++)
{
    var caption = track.Captions[i];
    await file.WriteLineAsync($"{caption.Text}");
}*/