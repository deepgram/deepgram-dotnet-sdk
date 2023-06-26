using System.Threading.Tasks;
using Deepgram.Models;
using Xunit;

namespace Deepgram.Tests.ClientTests
{

    public class PrerecordedTranscriptionTests
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Should_Return_PrerecordedTranscript_With_Id_Field_Populated()
        {
            var client = new DeepgramClient(new Credentials() { ApiKey = "SupplyID" });

            var uri = new UrlSource("https://static.deepgram.com/examples/Bueller-Life-moves-pretty-fast.wav");
            var callbackUri = "https://test.com";
            var options = new PrerecordedTranscriptionOptions { Callback = $"{callbackUri}" };

            var transcription = await client.Transcription.Prerecorded.GetTranscriptionAsync(uri, options);


            Assert.NotNull(transcription);
            Assert.NotNull(transcription.RequestID);

        }

    }
}
