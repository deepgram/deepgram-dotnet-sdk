namespace Deepgram.Tests.Fakes;
public static class MockHttpClient
{
    public static HttpClient CreateHttpClientWithResult<T>(
       T result, HttpStatusCode code = HttpStatusCode.OK, string url = $"https://{Constants.DEFAULT_URI}")
    {

        return new HttpClient(new MockHttpMessageHandler<T>(result, code))
        {
            BaseAddress = new Uri(url)
        };
    }

    public static HttpClient CreateHttpClientWithException<T>(
       T result, HttpStatusCode code = HttpStatusCode.OK)
    {
        return new HttpClient(new MockHttpMessageHandler<T>(result, code))
        {
            BaseAddress = new Uri($"https://{Constants.DEFAULT_URI}")
        };
    }
}
