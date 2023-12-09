using Deepgram.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Reflection;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", true)
    .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
    .Build();

var apiKey = configuration["deepgramApiKey"];

var httpClient = new HttpClient();
var options = new DeepgramClientOptions(apiKey);
var deepgramClient = new PrerecordedClient(options, httpClient);

var response = await deepgramClient.TranscribeUrlAsync(
    new UrlSource()
    {
        Url = "https://static.deepgram.com/examples/Bueller-Life-moves-pretty-fast.wav"
    },
    new PrerecordedSchema()
    {
        Punctuate = true,
        Utterances = true,
    });

Console.WriteLine(JsonConvert.SerializeObject(response));
Console.ReadKey();



