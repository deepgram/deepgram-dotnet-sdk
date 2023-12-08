namespace Deepgram.Tests.Fakes;
public class MockHttpMessageHandlerException(Exception exceptionType) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        throw exceptionType;
    }
}
