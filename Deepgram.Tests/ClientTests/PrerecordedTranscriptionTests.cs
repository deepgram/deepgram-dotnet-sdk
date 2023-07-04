using AutoBogus;
using Bogus;
using Deepgram.Request;
using Deepgram.Tests.Fakers;
using Deepgram.Tests.Fakes;
using Deepgram.Transcription;
using Xunit;

namespace Deepgram.Tests.ClientTests
{

    public class PrerecordedTranscriptionTests
    {
        readonly PrerecordedTranscriptionOptions _prerecordedTranscriptionOptions;
        readonly UrlSource _urlSource;
        public PrerecordedTranscriptionTests()
        {
            _prerecordedTranscriptionOptions = new PrerecordedTranscriptionOptions();
            _urlSource = new UrlSource(new Faker().Internet.Url());
        }

        [Fact]
        public async void GetTransaction_Should_Return_PrerecordedTranscription_When_UrlSource_Present()
        {
            //Arrange 
            var fakePrecordedTranscription = new AutoFaker<PrerecordedTranscription>().Generate();
            var SUT = GetDeepgramClient(fakePrecordedTranscription);

            //Act
            var result = await SUT.Transcription.Prerecorded.GetTranscriptionAsync(_urlSource, _prerecordedTranscriptionOptions);

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Results);
            Assert.IsAssignableFrom<PrerecordedTranscription>(result);
            Assert.Equal(fakePrecordedTranscription, result);
        }

        [Fact]
        public async void GetTransaction_Should_Return_PrerecordedTranscription_When_StreamSource_Present()
        {
            //Arrange 
            var fakePrecordedTranscription = new AutoFaker<PrerecordedTranscription>().Generate();
            var SUT = GetDeepgramClient(fakePrecordedTranscription);
            var fakeStreamSource = new StreamSourceFaker().Generate();

            //Act
            var result = await SUT.Transcription.Prerecorded.GetTranscriptionAsync(fakeStreamSource, _prerecordedTranscriptionOptions);

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Results);
            Assert.IsAssignableFrom<PrerecordedTranscription>(result);
            Assert.Equal(fakePrecordedTranscription, result);
        }

        [Fact]
        public async void Should_Return_A_Summary_Short_When_Summarize_Set_To_v2()
        {
            //Arrange
            PrerecordedTranscription responseObject = new AutoFaker<PrerecordedTranscription>().Generate();
            DeepgramClient SUT = GetDeepgramClient(responseObject);

            responseObject.Results.Summary.Short = "This is a test summary"!;


            var client = FakeHttpMessageHandler.CreateHttpClientWithResult(responseObject);
            var fakeOptions = new PrerecordedTranscriptionOptions()
            {
                Summarize = "v2"
            };
            SUT.Transcription.Prerecorded.ApiRequest = new ApiRequest(client);

            //Act
            var result = await SUT.Transcription.Prerecorded.GetTranscriptionAsync(_urlSource, fakeOptions);

            //Assert         
            Assert.NotNull(result);
            Assert.NotNull(result.Results.Summary.Short);

        }



        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void Should_Return_A_Summary_Short_When_Summarize_Set_To_bool(bool value)
        {
            //Arrange
            var responseObject = new AutoFaker<PrerecordedTranscription>().Generate();
            var SUT = GetDeepgramClient(responseObject);
            responseObject.Results.Summary.Short = null!;
            var client = FakeHttpMessageHandler.CreateHttpClientWithResult(responseObject);
            var fakeOptions = new PrerecordedTranscriptionOptions()
            {
                Summarize = value
            };

            SUT.Transcription.Prerecorded.ApiRequest = new ApiRequest(client);

            //Act
            var result = await SUT.Transcription.Prerecorded.GetTranscriptionAsync(_urlSource, fakeOptions);

            //Assert         
            Assert.NotNull(result);
            Assert.NotNull(result.Results);
            Assert.Null(result.Results.Summary.Short);
        }



        private static DeepgramClient GetDeepgramClient<T>(T returnObject)
        {
            var mockIRequestMessageBuilder = MockIRequestMessageBuilder.Create();
            var mockIApiRequest = MockIApiRequest.Create(returnObject);
            mockIApiRequest.SetupGet(x => x._requestMessageBuilder).Returns(mockIRequestMessageBuilder.Object);
            var credentials = new CredentialsFaker().Generate();
            var SUT = new DeepgramClient(credentials);
            SUT.Transcription.Prerecorded.ApiRequest = mockIApiRequest.Object;
            return SUT;
        }
    }
}