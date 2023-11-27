namespace Deepgram.Tests.Fakes;
public class MockHttpMessageHandler<T> : HttpMessageHandler
{
    private readonly T _response;
    private readonly HttpStatusCode _statusCode;
    private readonly bool _throwException;
    public MockHttpMessageHandler(T response, HttpStatusCode statusCode, bool throwException = false)
    {
        _response = response;
        _statusCode = statusCode;
        _throwException = throwException;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_throwException == true)
        {
            throw new HttpRequestException();
        }
        return new HttpResponseMessage()
        {
            StatusCode = _statusCode,
            Content = new StringContent(JsonSerializer.Serialize(_response))
        };


    }

}
