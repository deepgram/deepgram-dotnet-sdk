using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace Deepgram.Tests.Fakes
{
    public static class FakeHttpMessageHandler
    {
        public static Mock<HttpMessageHandler> CreateMessageHandlerWithResult<T>(
        T result, HttpStatusCode code = HttpStatusCode.OK)
        {
            var messageHandler = new Mock<HttpMessageHandler>();
            messageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = code,
                    Content = new StringContent(JsonConvert.SerializeObject(result)),
                });

            return messageHandler;
        }

        public static HttpClient CreateHttpClientWithResult<T>(
             T result, HttpStatusCode code = HttpStatusCode.OK)
        {
            var httpClient = new HttpClient(CreateMessageHandlerWithResult(result, code).Object)
            {
                BaseAddress = new Uri(FakeModels.FullUrl),
            };

            return httpClient;
        }
    }
}