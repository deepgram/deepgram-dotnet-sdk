using System.IO;
using AutoBogus;
using Deepgram.Models;
using Deepgram.Request;
using Deepgram.Tests.Fakes;

namespace Deepgram.Tests.ClientTests
{
    public class PrerecordedTranscriptionTests
    {
        private readonly PrerecordedTranscriptionOptions _prerecordedTranscriptionOptions;
        private readonly UrlSource _urlSource;
        private readonly Faker _faker = new();

        public PrerecordedTranscriptionTests()
        {
            _prerecordedTranscriptionOptions = new PrerecordedTranscriptionOptionsFaker().Generate();
            _urlSource = new UrlSource(new Faker().Internet.Url());
        }

        [Fact]
        public async Task GetTransaction_Should_Return_PrerecordedTranscription_When_UrlSource_Present()
        {
            //Arrange 
            var fakePrecordedTranscription = new AutoFaker<PrerecordedTranscription>().Generate();
            var SUT = GetDeepgramClient(fakePrecordedTranscription);
            _prerecordedTranscriptionOptions.Callback = null;

            //Act
            var result = await SUT.Transcription.Prerecorded.GetTranscriptionAsync(_urlSource, _prerecordedTranscriptionOptions);

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Results);
            Assert.IsAssignableFrom<PrerecordedTranscription>(result);
            Assert.Equal(fakePrecordedTranscription, result);
        }

        [Fact]
        public async Task GetTransaction_Should_Return_PrerecordedTranscription_When_StreamSource_Present()
        {
            //Arrange 
            var fakePrecordedTranscription = new AutoFaker<PrerecordedTranscription>().Generate();
            var SUT = GetDeepgramClient(fakePrecordedTranscription);
            var fakeStreamSource = new StreamSourceFaker().Generate();
            _prerecordedTranscriptionOptions.Callback = null;

            //Act
            var result = await SUT.Transcription.Prerecorded.GetTranscriptionAsync(fakeStreamSource, _prerecordedTranscriptionOptions);

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Results);
            Assert.IsAssignableFrom<PrerecordedTranscription>(result);
            Assert.Equal(fakePrecordedTranscription, result);
        }

        [Fact]
        public async Task Should_Return_A_Summary_Short_When_Summarize_Set_To_v2()
        {
            //Arrange
            var responseObject = new AutoFaker<PrerecordedTranscription>().Generate();
            var SUT = GetDeepgramClient(responseObject);

            responseObject.Results.Summary.Short = "This is a test summary";
            var client = MockHttpClient.CreateHttpClientWithResult(responseObject);
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
        public async Task Should_Return_A_Summary_Short_When_Summarize_Set_To_bool(bool value)
        {
            //Arrange
            var responseObject = new AutoFaker<PrerecordedTranscription>().Generate();
            var SUT = GetDeepgramClient(responseObject);
            responseObject.Results.Summary.Short = null;
            var fakeOptions = new PrerecordedTranscriptionOptions()
            {
                Summarize = value
            };

            //Act
            var result = await SUT.Transcription.Prerecorded.GetTranscriptionAsync(_urlSource, fakeOptions);

            //Assert         
            Assert.NotNull(result);
            Assert.NotNull(result.Results);
            Assert.Null(result.Results.Summary.Short);

        }

        [Fact]
        public async Task Should_Return_RequestId_When_Async_Transcription_From_Url_Requested()
        {
            //Arrange
            var responseObject = new AutoFaker<PrerecordedTranscriptionCallbackResult>().Generate();
            responseObject.RequestId = Guid.NewGuid();
            _prerecordedTranscriptionOptions.Callback = null;

            var SUT = GetDeepgramClient(responseObject);

            // Act
            var result = await SUT.Transcription.Prerecorded.GetTranscriptionAsync(_urlSource, _faker.Internet.Url(), _prerecordedTranscriptionOptions);

            // Assert

            Assert.Equal(responseObject.RequestId, result.RequestId);
        }

        [Fact]
        public async Task Should_Throw_When_Async_Transcription_From_Url_Has_No_Callback()
        {
            // Arrange
            var responseObject = new AutoFaker<string>().Generate();
            _prerecordedTranscriptionOptions.Callback = null;

            var SUT = GetDeepgramClient(responseObject);

            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
                await SUT.Transcription.Prerecorded.GetTranscriptionAsync(_urlSource,
                    null, _prerecordedTranscriptionOptions));

            Assert.Matches("CallbackUrl is required for this call. Please set the callbackUrl parameter or the callbackUrl property in the options object.", exception.Message);
        }

        [Fact]
        public async Task Should_Return_RequestId_When_Async_Transcription_From_Stream_Requested()
        {
            // Arrange
            var responseObject = new AutoFaker<PrerecordedTranscriptionCallbackResult>().Generate();
            responseObject.RequestId = Guid.NewGuid();
            _prerecordedTranscriptionOptions.Callback = null;

            var SUT = GetDeepgramClient(responseObject);

            using var stream = new MemoryStream(_faker.Random.Bytes(100));

            // Act
            var result = await SUT.Transcription.Prerecorded.GetTranscriptionAsync(new StreamSource(stream, "audio/wav"),
                               _faker.Internet.Url(), _prerecordedTranscriptionOptions);

            // Assert
            Assert.Equal(responseObject.RequestId, result.RequestId);
        }

        [Fact]
        public async Task Should_Throw_When_Async_Transcription_From_Stream_Has_No_Callback()
        {
            // Arrange
            var responseObject = new AutoFaker<PrerecordedTranscriptionCallbackResult>().Generate();
            responseObject.RequestId = Guid.NewGuid();
            _prerecordedTranscriptionOptions.Callback = null;

            var httpClient = MockHttpClient.CreateHttpClientWithResult(responseObject);

            var SUT = GetDeepgramClient(responseObject);
            SUT.Transcription.Prerecorded.ApiRequest = new ApiRequest(httpClient);

            using var stream = new MemoryStream(_faker.Random.Bytes(100));

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
                await SUT.Transcription.Prerecorded.GetTranscriptionAsync(_urlSource,
                    null, _prerecordedTranscriptionOptions));

            // Assert
            Assert.Matches("CallbackUrl is required for this call. Please set the callbackUrl parameter or the callbackUrl property in the options object.", exception.Message);
        }

        private static DeepgramClient GetDeepgramClient<T>(T returnObject)
        {
            var mockIRequestMessageBuilder = MockIRequestMessageBuilder.Create();
            var mockIApiRequest = MockIApiRequest.Create(returnObject);
            var credentials = new CredentialsFaker().Generate();
            var SUT = new DeepgramClient(credentials);
            SUT.Transcription.Prerecorded.ApiRequest = mockIApiRequest;
            SUT.Transcription.Prerecorded.RequestMessageBuilder = mockIRequestMessageBuilder;
            return SUT;
        }
    }
}
