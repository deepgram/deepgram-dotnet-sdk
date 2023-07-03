using Deepgram.Transcription;
using Newtonsoft.Json;

namespace SampleApp
{
    class Program
    {
        const string API_KEY = "YOUR_DEEPGRAM_API_KEY";

        static async Task Main(string[] args)
        {   
            var credentials = new Credentials(API_KEY);
            DeepgramClient deepgram = new DeepgramClient(credentials);

            // UNCOMMENT IF USING LOCAL FILE:
            // using (FileStream fs = File.OpenRead("preamble.wav"))
            {
                var response = await deepgram.Transcription.Prerecorded.GetTranscriptionAsync(
                    // UNCOMMENT IF USING LOCAL FILE:
                    // new Deepgram.Transcription.StreamSource(
                    //     fs,
                    //     "audio/wav"),
                    new Deepgram.Transcription.UrlSource("https://static.deepgram.com/examples/Bueller-Life-moves-pretty-fast.wav"),
                    new PrerecordedTranscriptionOptions()
                    {
                        Punctuate = true,
                        Utterances = true,
                    });

                Console.WriteLine(JsonConvert.SerializeObject(response));
            }
        }
    }
}