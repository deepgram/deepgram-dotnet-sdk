using AutoBogus;
using Bogus;
using Deepgram.Models;
using Deepgram.Request;
using Deepgram.Tests.Fakers;
using Deepgram.Tests.Fakes;
using System.IO;
using System.Threading.Tasks;
using System;
using Xunit;

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
            var SUT = GetDeepgramClient(responseObject);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await SUT.Transcription.Prerecorded.GetTranscriptionAsync(new UrlSource(_faker.Internet.Url()),
                    null, _prerecordedTranscriptionOptions));

            Assert.Matches("Callback is required", exception.Message);
        }

        [Fact]
        public async Task Should_Return_RequestId_When_Async_Transcription_From_Stream_Requested()
        {
            // Arrange
            var responseObject = new AutoFaker<PrerecordedTranscriptionCallbackResult>().Generate();
            responseObject.RequestId = Guid.NewGuid();

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
            var httpClient = FakeHttpMessageHandler.CreateHttpClientWithResult(responseObject);

            var SUT = GetDeepgramClient(responseObject);
            SUT.Transcription.Prerecorded.ApiRequest = new ApiRequest(httpClient);

            using var stream = new MemoryStream(_faker.Random.Bytes(100));

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await SUT.Transcription.Prerecorded.GetTranscriptionAsync(new UrlSource(_faker.Internet.Url()),
                    null, _prerecordedTranscriptionOptions));

            // Assert
            Assert.Matches("Callback is required", exception.Message);
        }

        private static DeepgramClient GetDeepgramClient<T>(T returnObject)
        {
            var mockIRequestMessageBuilder = MockIRequestMessageBuilder.Create();
            var mockIApiRequest = MockIApiRequest.Create(returnObject);
            var credentials = new CredentialsFaker().Generate();
            var SUT = new DeepgramClient(credentials);
            SUT.Transcription.Prerecorded.ApiRequest = mockIApiRequest.Object;
            SUT.Transcription.Prerecorded.RequestMessageBuilder = mockIRequestMessageBuilder.Object;
            return SUT;
        }
    }
}
