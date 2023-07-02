namespace Deepgram.Tests.Fakes;

public class MockIApiRequest
{
    public static Mock<IApiRequest> Create<T>(T returnObject)
    {
        var mockAPiRequest = new Mock<IApiRequest>();
        mockAPiRequest.Setup(x => x.SendHttpRequestAsync<T>(It.IsAny<HttpRequestMessage>())).ReturnsAsync(returnObject);
        return mockAPiRequest;
    }
}
