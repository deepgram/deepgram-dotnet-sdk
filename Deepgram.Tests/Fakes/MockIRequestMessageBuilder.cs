using Deepgram.Models;


namespace Deepgram.Tests.Fakes
{
    public class MockIRequestMessageBuilder
    {
        public static IRequestMessageBuilder Create()
        {
            var mockRequestMessageBuilder = Substitute.For<IRequestMessageBuilder>();
            mockRequestMessageBuilder.CreateHttpRequestMessage(
                Arg.Any<HttpMethod>(),
                Arg.Any<string>(),
                Arg.Any<Credentials>(),
                Arg.Any<object>(),
                Arg.Any<object>()).Returns(new HttpRequestMessage());
            return mockRequestMessageBuilder;
        }
    }
}
