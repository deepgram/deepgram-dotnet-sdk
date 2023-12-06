namespace Deepgram.Tests.Fakes;
public class MockHttpMessageHandler<T>(T response, HttpStatusCode statusCode) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(new HttpResponseMessage()
        {
            StatusCode = statusCode,
            Content = new StringContent(JsonSerializer.Serialize(response))
        });

}
