using Deepgram.Transcription;

namespace SampleApp
{
    class Program
    {
        const string API_KEY = "DEEPGRAM_API_KEY";

        static async Task Main(string[] args)
        {
            DeepgramClient deepgram = new DeepgramClient(new Credentials(API_KEY));
            var response = await deepgram.Transcription.Prerecorded.GetTranscriptionAsync(
                    new UrlSource("https://static.deepgram.com/examples/Bueller-Life-moves-pretty-fast.wav"),
                    new PrerecordedTranscriptionOptions()
                    {
                        Punctuate = true,
                        Utterances = true,
                        Redaction = new [] { "pci", "ssn" }
                    });

            Console.Write(response.ToWebVTT());
        }
    }
}