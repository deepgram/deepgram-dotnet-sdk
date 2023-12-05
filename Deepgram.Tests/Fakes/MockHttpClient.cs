namespace Deepgram.Tests.Fakes;
public static class MockHttpClient
{
    public static HttpClient CreateHttpClientWithResult<T>(
       T result, HttpStatusCode code = HttpStatusCode.OK, string? url = $"https://{Constants.DEFAULT_URI}")
    {

        var httpClient = new HttpClient(new MockHttpMessageHandler<T>(result, code))
        {
            BaseAddress = new Uri(url!)
        };

        return httpClient;
    }

    public static HttpClient CreateHttpClientWithException<T>(
       T result, HttpStatusCode code = HttpStatusCode.OK)
    {

        var httpClient = new HttpClient(new MockHttpMessageHandler<T>(result, code))
        {
            BaseAddress = new Uri($"https://{Constants.DEFAULT_URI}")
        };

        return httpClient;
    }
}
