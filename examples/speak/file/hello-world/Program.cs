using Deepgram;
using Deepgram.Models.Speak.v1;
using System.Text.Json;

namespace SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
            var deepgramClient = new SpeakClient();

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