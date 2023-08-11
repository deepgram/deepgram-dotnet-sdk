
namespace Deepgram.Tests.Fakes
{
    public static class MockHttpClient
    {
        public static HttpClient CreateHttpClientWithResult<T>(
           T result, HttpStatusCode code = HttpStatusCode.OK)
        {
            var httpClient = new HttpClient(new MockHttpMessageHandler(result, code))
            {
                BaseAddress = new Uri(new Faker().Internet.Url()),
            };

            return httpClient;
        }
    }
}