using System.Net;
using System.Web;
using Bogus;
using Codenizer.HttpClient.Testable;
using Deepgram.Request;
using Deepgram.Transcription;
using Newtonsoft.Json;

namespace Deepgram.Tests.Transcription
{
    public class PrerecordedTranscriptionClientTests : IDisposable
    {
        private readonly TestableMessageHandler _testHandler = new();
        private readonly Faker _faker = new ();
        private readonly HttpMessageHandler _defaultHandler;

        public PrerecordedTranscriptionClientTests()
        {
            _defaultHandler = Configuration.Instance.ClientHandler;
            Configuration.Instance.ClientHandler = _testHandler;
        }

        [Fact]
        public async Task GetTranscriptionFromUrlWithCallbackAsyncShouldMakeRequestWithCallbackReturnRequestId()
        {
            var client = CreateClient();
            var requestId = Guid.NewGuid().ToString();
            var callback = _faker.Internet.Url();
            _testHandler.RespondTo()
                .Post()
                .ForUrl("/v1/listen?callback=*")
                .WithQueryStringParameter("callback")
                .HavingValue(HttpUtility.UrlEncode(callback))
                .With(HttpStatusCode.OK)
                .AndContent("application/json",
                    JsonConvert.SerializeObject(new PrerecordedTranscriptionRequest() { Id = requestId }));

            var result = await client.GetTranscriptionWithCallbackAsync(new UrlSource(_faker.Internet.Url()),
                new PrerecordedTranscriptionOptionsWithCallback() { Callback = callback });
            
            Assert.Equal(requestId, result.Id);
        }
        
        [Fact]
        public async Task GetTranscriptionFromUrlWithCallbackAsyncShouldThrowWhenNoCallbackProvided()
        {
            var client = CreateClient();

            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
                await client.GetTranscriptionWithCallbackAsync(new UrlSource(_faker.Internet.Url()),
                    new PrerecordedTranscriptionOptionsWithCallback() { Callback = null }));
    
            Assert.Matches("Callback is required", exception.Message);
        }
        
        [Fact]
        public async Task GetTranscriptionFromStreamWithCallbackAsyncShouldMakeRequestWithCallbackReturnRequestId()
        {
            var client = CreateClient();
            var requestId = Guid.NewGuid().ToString();
            var callback = _faker.Internet.Url();
            _testHandler.RespondTo()
                .Post()
                .ForUrl("/v1/listen?callback=*")
                .WithQueryStringParameter("callback")
                .HavingValue(HttpUtility.UrlEncode(callback))
                .AndContentType("audio/wav")
                .With(HttpStatusCode.OK)
                .AndContent("application/json",
                    JsonConvert.SerializeObject(new PrerecordedTranscriptionRequest() { Id = requestId }));
            using var stream = new MemoryStream(_faker.Random.Bytes(100));

            var result = await client.GetTranscriptionWithCallbackAsync(new StreamSource(stream, "audio/wav"),
                new PrerecordedTranscriptionOptionsWithCallback() { Callback = callback });
            
            Assert.Equal(requestId, result.Id);
        }
        
        [Fact]
        public async Task GetTranscriptionFromStreamWithCallbackAsyncShouldThrowWhenNoCallbackProvided()
        {
            var client = CreateClient();
            using var stream = new MemoryStream();

            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
                await client.GetTranscriptionWithCallbackAsync(new StreamSource(stream, "audio/wav"),
                    new PrerecordedTranscriptionOptionsWithCallback() { Callback = null }));
    
            Assert.Matches("Callback is required", exception.Message);
        }

        public void Dispose()
        {
            Configuration.Instance.ClientHandler = _defaultHandler;
            _testHandler.Dispose();
        }

        private PrerecordedTranscriptionClient CreateClient()
        {
            return new PrerecordedTranscriptionClient(new CleanCredentials(_faker.Internet.Password(),
                _faker.Internet.DomainName(), true));
        }
    }
}