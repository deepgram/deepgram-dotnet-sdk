using Deepgram.Models;
using Xunit;

namespace Deepgram.Tests
{
    public class SummarizeTest
    {
        [Fact]
        public async void Should_Return_A_Summary_Short_When_Summarize_Set_To_v2()
        {
            var creds = new Credentials()
            {
                ApiKey = "",
                ApiUrl = "api.beta.deepgram.com",
                RequireSSL = true,
            };
            var client = new DeepgramClient(creds);
            var uri = new UrlSource("https://static.deepgram.com/examples/Bueller-Life-moves-pretty-fast.wav");

            var options = new PrerecordedTranscriptionOptions { Summarize = "v2" };

            var transcription = await client.Transcription.Prerecorded.GetTranscriptionAsync(uri, options);


            Assert.NotNull(transcription);
            Assert.NotNull(transcription.Results.Summary.Short);
        }
    }
}

