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


//Console.WriteLine($"The amount of slices is {slices}");

using StreamWriter file = new("WriteLines.txt", append: true);


//Console.WriteLine($"The tile is: {title}, it was created by {author} and it is {duration} long");

var trackManifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(
    youtubeUrl
);

// Find closed caption track in English
var trackInfo = trackManifest.GetByLanguage("en");

var track = await youtube.Videos.ClosedCaptions.GetAsync(trackInfo);

Console.WriteLine($"The amount of captions is {track.Captions.Count}");

var slices = duration.Value.TotalMinutes / 5; // 11.6444

int sliceCount = Convert.ToInt32(Math.Floor(slices)); // 11 left over .6444
int startIndex = 0;

double captionIndex = track.Captions.Count / sliceCount;

int captionSlices = Convert.ToInt32(Math.Floor(captionIndex));

Console.WriteLine($"The amount of captionSlices per slice is {captionSlices}");
Console.WriteLine($"The amount of slices is {sliceCount}");

//int endIndex = captionSlices;

/*for (int i = 0; i < sliceCount; i++)
{
    var caption = track.Captions[startIndex];
    await file.WriteLineAsync($"TIMESTAMP: {caption.Offset}");
    
    for ( var j = startIndex; j <= endIndex; j++)
    {
        caption = track.Captions[j];
        await file.WriteLineAsync($"{caption.Text}");
        //Console.WriteLine($"From: {caption.Text}");
    }

    startIndex = startIndex+(captionSlices/5);
    endIndex = endIndex+(captionSlices/5);
}*/
// Get the caption displayed at 0:35
//var caption = track.GetByTime(TimeSpan.FromMinutes(15));
//var text = caption.Text; // "collection acts as the parent collection"

//Console.WriteLine(text);

int index = 0;
int endIndex = track.Captions.Count / sliceCount;

for (int l = 0; l < sliceCount; l++)
{
    var caption = track.Captions[l];
    await file.WriteLineAsync($"TIMESTAMP: {caption.Offset}");
    Console.WriteLine($"Starting: {index} Ending: {endIndex}");

    for (int k = index; k < endIndex; k++)
    {

        caption = track.Captions[k];
        //await file.WriteLineAsync($"TIMESTAMP: {caption.Offset}");
        await file.WriteLineAsync($"{caption.Text}");
    }

if (endIndex+captionSlices < track.Captions.Count){
index = index + captionSlices;
        endIndex = endIndex + captionSlices;
}
    
        
    
    
}

Console.WriteLine($"LASt Starting: {index} Ending: {endIndex}");

for (var m = endIndex; m < track.Captions.Count; m++)
{
    var caption = track.Captions[m];
    //caption = track.Captions[k];
        //await file.WriteLineAsync($"TIMESTAMP: {caption.Offset}");
        await file.WriteLineAsync($"{caption.Text}");
}


// how to get captions for 5 min chunks
// when to best call api?
// do i need transcript file?
// how to get transcript file?