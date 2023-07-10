using System.Net.Http;
using System.Threading.Tasks;
using Deepgram.Request;

namespace Deepgram.Tests.Fakes;
internal class TestableApiRequest : ApiRequest
{
    public object response;
    public TestableApiRequest(HttpClient httpClient, CleanCredentials credentials) : base(httpClient, credentials)
    {
    }

    internal override async Task<T> SendHttpRequestAsync<T>(HttpRequestMessage message)
    {
        return await Task.FromResult((T)response);
    }


}
