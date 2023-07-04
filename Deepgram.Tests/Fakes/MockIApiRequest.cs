using System.Net.Http;
using Deepgram.Request;
using Deepgram.Transcription;
using Moq;

namespace Deepgram.Tests.Fakes
{

    public class MockIApiRequest
    {
        internal static Mock<IApiRequest> Create<T>(T returnObject)
        {
            var mockAPiRequest = new Mock<IApiRequest>();
            mockAPiRequest.Setup(x => x.DoRequestAsync<T>(
                It.IsAny<HttpMethod>(),
                    It.IsAny<string>(),
                    It.IsAny<CleanCredentials>(),
                    It.IsAny<object>(),
                It.IsAny<object>())).ReturnsAsync(returnObject);

            mockAPiRequest.Setup(x => x.DoStreamRequestAsync<T>(
                It.IsAny<HttpMethod>(),
                    It.IsAny<string>(),
                    It.IsAny<CleanCredentials>(),
                    It.IsAny<StreamSource>(),
                It.IsAny<object>())).ReturnsAsync(returnObject);
            return mockAPiRequest;
        }


    }
}