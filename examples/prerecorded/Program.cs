using Deepgram.Models;
using Deepgram.Models.Options;
using Newtonsoft.Json;

namespace SampleApp;
public static class Program
{
    private const string API_KEY = "YOUR_DEEPGRAM_API_KEY";

    public static async Task Main()
    {
        var httpClientFactory = new HttpClientFactory(); // Brute force. Don't do this, use dependency injection.
        var options = new DeepgramClientOptions("apiKey");
        var deepgramClient = new PrerecordedClient(httpClientFactory, options);

        var response = await deepgramClient.TranscribeUrlAsync(
            new UrlSource()
            {
                Url = "https://static.deepgram.com/examples/Bueller-Life-moves-pretty-fast.wav"
            },
            new Deepgram.Models.Schemas.PrerecordedSchema()
            {
                Punctuate = true,
                Utterances = true,
            });

        Console.WriteLine(JsonConvert.SerializeObject(response));
    }

    // Brute force. Don't do this, use dependency injection.
    private class HttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient();
        }
    }
}

