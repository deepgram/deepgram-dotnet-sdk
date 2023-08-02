using Deepgram.Clients;
using Deepgram.Models;
using Newtonsoft.Json;

namespace SampleApp
{
    class Program
    {

        static async Task Main(string[] args)
        {
            var credentials = new Credentials("");
            var deepgramClient = new DeepgramClient(credentials);
            var response = await deepgramClient.Transcription.Prerecorded.GetTranscriptionAsync(
                    new UrlSource("https://static.deepgram.com/examples/nasa-spacewalk-interview.wav"),
                    new PrerecordedTranscriptionOptions()
                    {
                        Tier = "nova",
                        FillerWords = true
                    });

            Console.WriteLine(JsonConvert.SerializeObject(response));
        }
    }
}