using System.Net.Http;
using Deepgram.Models;
using Deepgram.Request;
using Xunit;

namespace Deepgram.Tests.RequestTests
{
    public class RequestMessageBuilderTests
    {
        Credentials Credentials;

        public RequestMessageBuilderTests()
        {
            Credentials = FakeModels.Credentials;
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateHttpRequestMessage_Should_Return_HttpRequestMessage_When_HttpMethod_Is_Get_No_QueryParameters(bool requireSSL)
        {
            //Arrange            
            Credentials.RequireSSL = requireSSL;

            //Act
            var result = RequestMessageBuilder.CreateHttpRequestMessage(HttpMethod.Get, FakeModels.UriSegment, Credentials);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<HttpRequestMessage>(result);
            Assert.Equal(HttpMethod.Get, result.Method);
            Assert.Equal(Credentials.ApiKey, result.Headers.Authorization.Parameter);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateHttpRequestMessage_Should_Return_HttpRequestMessage_When_HttpMethod_Is_Get_With_QueryParameters(bool requireSSL)
        {
            //Arrange
            Credentials.RequireSSL = requireSSL;

            //Act
            var result = RequestMessageBuilder.CreateHttpRequestMessage(HttpMethod.Get, FakeModels.UriSegment, Credentials, null, FakeModels.PrerecordedTranscriptionOptions);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<HttpRequestMessage>(result);
            Assert.Equal(HttpMethod.Get, result.Method);
            Assert.Equal(Credentials.ApiKey, result.Headers.Authorization.Parameter);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateHttpRequestMessage_Should_Return_HttpRequestMessage_When_HttpMethod_Is_Post_With_Body(bool requireSSL)
        {
            //Arrange
            Credentials.RequireSSL = requireSSL;

            //Act
            var result = RequestMessageBuilder.CreateHttpRequestMessage(HttpMethod.Post, FakeModels.UriSegment, Credentials, FakeModels.UrlSource, FakeModels.PrerecordedTranscriptionOptions);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<HttpRequestMessage>(result);
            var content = result.Content;
            Assert.NotNull(result.Content);
            Assert.Equal(HttpMethod.Post, result.Method);
            Assert.Equal(Credentials.ApiKey, result.Headers.Authorization.Parameter);
            Assert.Equal("application/json", content.Headers.ContentType.MediaType);

        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateHttpRequestMessage_Should_Return_HttpRequestMessage_When_HttpMethod_Is_Put_With_Body(bool requireSSL)
        {
            //Arrange
            Credentials.RequireSSL = requireSSL;

            //Act
            var result = RequestMessageBuilder.CreateHttpRequestMessage(HttpMethod.Put, FakeModels.UriSegment, Credentials, FakeModels.UpdateScopeOptions, null);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<HttpRequestMessage>(result);
            var content = result.Content;
            Assert.NotNull(result.Content);
            Assert.Equal(HttpMethod.Put, result.Method);
            Assert.Equal(Credentials.ApiKey, result.Headers.Authorization.Parameter);
            Assert.Equal("application/json", content.Headers.ContentType.MediaType);

        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateHttpRequestMessage_Should_Return_HttpRequestMessage_When_HttpMethod_Is_Patch_With_Body(bool requireSSL)
        {
            //Arrange
            Credentials.RequireSSL = requireSSL;
            //Act

#if NETSTANDARD2_0
            var result = RequestMessageBuilder.CreateHttpRequestMessage(new HttpMethod("PATCH"), FakeModels.UriSegment, Credentials, FakeModels.Project, null);
#else            
            var result = RequestMessageBuilder.CreateHttpRequestMessage(HttpMethod.Patch, FakeModels.UriSegment, Credentials, FakeModels.Project, null);
#endif


            //Assert
            Assert.NotNull(result);
            Assert.IsType<HttpRequestMessage>(result);
            var content = result.Content;
            Assert.NotNull(result.Content);
#if NETSTANDARD2_0
            Assert.Equal(new HttpMethod("PATCH"), result.Method);
#else
            Assert.Equal(HttpMethod.Patch, result.Method);

#endif
            Assert.Equal(Credentials.ApiKey, result.Headers.Authorization.Parameter);
            Assert.Equal("application/json", content.Headers.ContentType.MediaType);

        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateStreamHttpRequestMessage_Should_Return_HttpRequestMessage_When_HttpMethod_Is_Post_With_Body(bool requireSSL)
        {
            //Arrange
            Credentials.RequireSSL = requireSSL;

            //Act
            var result = RequestMessageBuilder.CreateStreamHttpRequestMessage(HttpMethod.Post, FakeModels.UriSegment, Credentials, FakeModels.StreamSource);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<HttpRequestMessage>(result);
            var content = result.Content;
            Assert.NotNull(result.Content);
            Assert.Equal(HttpMethod.Post, result.Method);
            Assert.Equal(Credentials.ApiKey, result.Headers.Authorization.Parameter);
            Assert.Equal("text/plain", content.Headers.ContentType.MediaType);

        }


    }
}