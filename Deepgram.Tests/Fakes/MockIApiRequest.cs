namespace Deepgram.Tests.Fakes
{
    public class MockIApiRequest
    {
        public static IApiRequest Create<T>(T returnObject)
        {
            var mockAPiRequest = Substitute.For<IApiRequest>();
            mockAPiRequest.SendHttpRequestAsync<T>(Arg.Any<HttpRequestMessage>()).Returns(returnObject);
            return mockAPiRequest;
        }
    }
}
