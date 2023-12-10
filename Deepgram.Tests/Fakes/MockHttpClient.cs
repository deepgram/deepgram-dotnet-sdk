using Deepgram.Constants;

namespace Deepgram.Tests.Fakes;
public static class MockHttpClient
{
    public static HttpClient CreateHttpClientWithResult<T>(
       T result, HttpStatusCode code = HttpStatusCode.OK, string url = $"https://{Defaults.DEFAULT_URI}")
    {

        return new HttpClient(new MockHttpMessageHandler<T>(result, code))
        {
            BaseAddress = new Uri(url)
        };
    }

    public static HttpClient CreateHttpClientWithException(Exception Exception)
    {
        return new HttpClient(new MockHttpMessageHandlerException(Exception))
        {
            BaseAddress = new Uri($"https://{Defaults.DEFAULT_URI}")
        };
    }
}
