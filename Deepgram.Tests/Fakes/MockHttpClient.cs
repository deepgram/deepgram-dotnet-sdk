namespace Deepgram.Tests.Fakes;
public static class MockHttpClient
{
    public static HttpClient CreateHttpClientWithResult<T>(
       T result, HttpStatusCode code = HttpStatusCode.OK, string? url = null)
    {
        Uri? baseAddress = null;
        if (url is null)
        {
            baseAddress = new Uri(new Faker().Internet.Url());
        }
        var httpClient = new HttpClient(new MockHttpMessageHandler<T>(result, code))
        {
            BaseAddress = baseAddress
        };

        return httpClient;
    }
}
