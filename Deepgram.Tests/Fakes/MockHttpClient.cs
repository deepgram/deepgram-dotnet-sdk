namespace Deepgram.Tests.Fakes;
public static class MockHttpClient
{
    public static HttpClient CreateHttpClientWithResult<T>(
       T result, HttpStatusCode code = HttpStatusCode.OK, string? url = Constants.DEFAULT_URI, bool throwException = false)
    {

        var httpClient = new HttpClient(new MockHttpMessageHandler<T>(result, code, throwException))
        {
            BaseAddress = new Uri(url)
        };

        return httpClient;
    }

    public static HttpClient CreateHttpClientWithException<T>(
       T result, HttpStatusCode code = HttpStatusCode.OK)
    {

        var httpClient = new HttpClient(new MockHttpMessageHandler<T>(result, code, true))
        {
            BaseAddress = new Uri(Constants.DEFAULT_URI)
        };

        return httpClient;
    }
}
