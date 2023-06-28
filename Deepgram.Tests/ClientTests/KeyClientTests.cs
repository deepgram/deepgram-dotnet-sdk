using System.Net.Http;
using Deepgram.Interfaces;
using Deepgram.Models;
using Moq;
using Xunit;

namespace Deepgram.Tests.ClientTests
{
    public class KeyClientTests
    {
        [Fact]
        public async void Should()
        {
            var mockRequestMessageBuilder = CreateMockRequestMessageBuilder();

            var mockAPiRequest = CreateMockIApiRequest<KeyList>(null);
        }


        private static Mock<IRequestMessageBuilder> CreateMockRequestMessageBuilder()
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

        private static Mock<IApiRequest> CreateMockIApiRequest<T>(T returnObject)
        {
            var mockAPiRequest = new Mock<IApiRequest>();
            mockAPiRequest.Setup(x => x.SendHttpRequestAsync<T>(It.IsAny<HttpRequestMessage>())).ReturnsAsync(returnObject);
            return mockAPiRequest;
        }
    }
}
