using Deepgram;
using Deepgram.Models.PreRecorded.v1;
using System.Text.Json;

namespace PreRecorded
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
            var deepgramClient = new PreRecordedClient();

            var response = await deepgramClient.TranscribeUrl(
                new UrlSource("https://static.deepgram.com/examples/Bueller-Life-moves-pretty-fast.wav"),
                new PrerecordedSchema()
                {
                    Model = "nova-2",
                });

            //Console.WriteLine(response);
            Console.WriteLine(JsonSerializer.Serialize(response));
            Console.ReadKey();
        }
    }
}