using System;
using Deepgram.Interfaces;
using Deepgram.Tests.Fakes;
using Deepgram.Utilities;
using Xunit;

namespace Deepgram.Tests.ClientTests
{
    public class DeepClientTests
    {
        [Fact]
        public void Should_Throw_Exception_When_No_Apikey_Present()
        {
            //Act
            var ex = Assert.Throws<ArgumentException>(() => _ = new DeepgramClient());

            //Assert
            Assert.IsAssignableFrom<ArgumentException>(ex);
            Assert.Equal("Deepgram API Key must be provided in constructor", ex.Message);
        }


        [Fact]
        public void Should_Initialize_Clients()
        {

            //Act
            var result = new DeepgramClient(FakeModels.Credentials);

            //Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IKeyClient>(result.Keys);
            Assert.IsAssignableFrom<IProjectClient>(result.Projects);
            Assert.IsAssignableFrom<ITranscriptionClient>(result.Transcription);
            Assert.IsAssignableFrom<IUsageClient>(result.Usage);
        }

        [Fact]
        public void Should_Initialize_LiveTranscriptionClient()
        {
            //Arrange

            var SUT = new DeepgramClient(FakeModels.Credentials);
            //Act

            var result = SUT.CreateLiveTranscriptionClient();

            //Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<ILiveTranscriptionClient>(result);

        }


        [Theory]
        [InlineData(2)]
        [InlineData(10)]
        [InlineData(20)]
        [InlineData(30)]
        public void Should_Set_TimeOut_On_HttpClient(double timeSpan)
        {

            //Arrange
            var SUT = new DeepgramClient(FakeModels.Credentials);

            //Act
            SUT.SetHttpClientTimeout(TimeSpan.FromSeconds(timeSpan));
            var httpClient = HttpClientUtil.HttpClient;

            //Assert
            Assert.NotNull(httpClient);
            Assert.Equal(timeSpan, httpClient.Timeout.TotalSeconds);
        }


    }
}
