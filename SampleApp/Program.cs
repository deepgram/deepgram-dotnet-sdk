using Deepgram.Transcription;
using Newtonsoft.Json;

namespace SampleApp
{
    class Program
    {
        const string API_KEY = "";

        static async Task Main(string[] args)
        {
            DeepgramClient deepgram = new DeepgramClient(new Credentials(API_KEY));
            var response = await deepgram.Transcription.Prerecorded.GetTranscriptionAsync(
                    new UrlSource("https://www.happyhourspanish.com/wp-content/uploads/podcasts/HHS_Podcast_Soccer_Eurocup.mp3"),
                    new PrerecordedTranscriptionOptions()
                    {
                        Summarize = "v2",
                        DetectLanguage = true
                    });

            Console.WriteLine(JsonConvert.SerializeObject(response));
        }
    }
}