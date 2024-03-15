using Deepgram;
using Deepgram.Models.PreRecorded.v1;
using System.Text.Json;

namespace SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var apiKey = "REPLACE-WITH-YOUR-API-KEY";
            var deepgramClient = new PreRecordedClient(apiKey);

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