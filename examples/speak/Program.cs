using Deepgram;
using Deepgram.Models.Speak.v1;
using System.Text.Json;

namespace SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Replace "REPLACE-WITH-YOUR-API-KEY" with your actual Deepgram API key
            var apiKey = "REPLACE-WITH-YOUR-API-KEY";
            var deepgramClient = new SpeakClient(apiKey);

            var response = await deepgramClient.ToFile(
                new TextSource("Hello World!"),
                "test.mp3",
                new SpeakSchema()
                {
                    Model = "aura-asteria-en",
                });

            //Console.WriteLine(response);
            Console.WriteLine(JsonSerializer.Serialize(response));
            Console.ReadKey();
        }
    }
}