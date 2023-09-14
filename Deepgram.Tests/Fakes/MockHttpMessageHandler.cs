#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

using Newtonsoft.Json;

namespace Deepgram.Tests.Fakes;
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly object _response;
    private readonly HttpStatusCode _statusCode;
    public MockHttpMessageHandler(object response, HttpStatusCode statusCode)
    {
        _response = response;
        _statusCode = statusCode;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return new HttpResponseMessage()
        {
            StatusCode = _statusCode,
            Content = new StringContent(JsonConvert.SerializeObject(_response))
        };
    }
}
