// See https://aka.ms/new-console-template for more information

// https://www.nuget.org/packages/YoutubeExplode

using YoutubeExplode;

string youtubeUrl = "https://www.youtube.com/watch?v=LgWRbXw9dRU";

var youtube = new YoutubeClient();

// You can specify either video ID or URL
var video = await youtube.Videos.GetAsync(youtubeUrl);

var title = video.Title; // "Collections - Blender 2.80 Fundamentals"
var author = video.Author.ChannelTitle; // "Blender"
var duration = video.Duration; // 00:07:20
var slices = duration.Value.TotalMinutes / 5;

Console.WriteLine($"The amount of slices is {slices}");

using StreamWriter file = new("WriteLines2.txt", append: true);


Console.WriteLine($"The tile is: {title}, it was created by {author} and it is {duration} long");

var trackManifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(
    youtubeUrl
);

// Find closed caption track in English
var trackInfo = trackManifest.GetByLanguage("en");

var track = await youtube.Videos.ClosedCaptions.GetAsync(trackInfo);

 int interval = Convert.ToInt32(Math.Floor(slices));

for (int j = 0; j < slices; j++)
{
    var caption = track.Captions[j];

    await file.WriteLineAsync($"TIMESTAMP: {caption.Offset}");

    for (var i = 0; i < track.Captions.Count; i+=interval)
    {
        //var caption = track.Captions[i];
        var newcaption = track.Captions[i];
        await file.WriteLineAsync($"{newcaption.Text}");
        //Console.WriteLine($"From: {caption.Text}");
    }
}
// Get the caption displayed at 0:35
//var caption = track.GetByTime(TimeSpan.FromMinutes(15));
//var text = caption.Text; // "collection acts as the parent collection"

//Console.WriteLine(text);


// how to get captions for 5 min chunks
// when to best call api?
// do i need transcript file?
// how to get transcript file?