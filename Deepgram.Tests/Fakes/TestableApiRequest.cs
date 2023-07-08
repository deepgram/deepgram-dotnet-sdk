namespace Deepgram.Tests.Fakes;
internal class TestableApiRequest : ApiRequest
{
    public object response;
    public TestableApiRequest(HttpClient httpClient, CleanCredentials credentials) : base(httpClient, credentials)
    {
    }

    internal override async Task<T> SendHttpRequestAsync<T>(HttpMethod method, string uri, object? body = null, object? queryParameters = null)
    {
        return (T)response;
    }


}
