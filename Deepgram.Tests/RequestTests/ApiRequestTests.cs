using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Deepgram.Models;
using Deepgram.Request;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;

namespace Deepgram.Tests.RequestTests
{
    public class ApiRequestTests
    {
        [Fact]

        public async void Should()
        {
            //Arrange
            var responseObject = new Project() { Company = "testCompany", Id = "fakeId", Name = "fakeName" };
            var client = CreateHttpClientWithResult(responseObject);
            var SUT = new ApiRequest(client);

            //Act
            var result = await SUT.SendHttpRequestAsync<Project>(new HttpRequestMessage());

            //Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<Project>(result);
        }

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
                BaseAddress = new Uri("https://api-client-under-test.com"),
            };

            return httpClient;
        }



    }
}