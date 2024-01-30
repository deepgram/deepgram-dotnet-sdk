using Deepgram;
using Deepgram.Models;


var apiKey = "deepgramApiKey";

var deepgramClient = new PrerecordedClient(apiKey);

var response = await deepgramClient.TranscribeUrl(
    new UrlSource("https://static.deepgram.com/examples/Bueller-Life-moves-pretty-fast.wav"),
    new PrerecordedSchema()
    {
        Punctuate = true,
        Utterances = true,
    });

Console.WriteLine(response);
Console.ReadKey();



