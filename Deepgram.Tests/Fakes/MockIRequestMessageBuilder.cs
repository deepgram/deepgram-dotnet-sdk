using System.Net.Http;
using Deepgram.Interfaces;
using Deepgram.Models;
using Moq;

namespace Deepgram.Tests.Fakes
{
    public class MockIRequestMessageBuilder
    {
        public static Mock<IRequestMessageBuilder> Create()
        {
            var mockRequestMessageBuilder = new Mock<IRequestMessageBuilder>();
            mockRequestMessageBuilder.Setup(x => x.CreateHttpRequestMessage(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                It.IsAny<Credentials>(),
                It.IsAny<object>(),
                It.IsAny<object>())).Returns(new HttpRequestMessage());
            return mockRequestMessageBuilder;
        }
    }
}
