using System.IO;
using System.Net.Http;
using Deepgram.Models;
using Deepgram.Request;
using Xunit;

namespace Deepgram.Tests.ApiRequestTests
{
    public class HttpRequestMessageBuilderTests
    {
        Credentials Credentials;
        string Segment;
        public HttpRequestMessageBuilderTests()
        {
            Credentials = new Credentials()
            {
                ApiKey = "apikey",
                ApiUrl = "apiurl.com"
            };

            Segment = "test";
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConfigureHttpRequestMessage_Should_Return_HttpRequestMessage_When_HttpMethod__Is_Get(bool requireSSL)
        {
            //Arrange
            Credentials.RequireSSL = requireSSL;

            //Act
            var result = HttpRequestMessageBuilder.ConfigureHttpRequestMessage(HttpMethod.Get, Segment, Credentials);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<HttpRequestMessage>(result);
            Assert.Equal(HttpMethod.Get, result.Method);
            Assert.Equal(Credentials.ApiKey, result.Headers.Authorization.Parameter);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConfigureHttpRequestMessage_Should_Return_HttpRequestMessage_When_HttpMethod_Is_Post(bool requireSSL)
        {
            //Arrange
            Credentials.RequireSSL = requireSSL;

            //Act
            var result = HttpRequestMessageBuilder.ConfigureHttpRequestMessage(HttpMethod.Post, Segment, Credentials);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<HttpRequestMessage>(result);
            Assert.Equal(HttpMethod.Post, result.Method);
            Assert.Equal(Credentials.ApiKey, result.Headers.Authorization.Parameter);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConfigureHttpRequestMessage_Should_Return_HttpRequestMessage_When_HttpMethod_Is_Patch(bool requireSSL)
        {
            //Arrange
            Credentials.RequireSSL = requireSSL;

            //Act
#if NETSTANDARD2_0
             var result = HttpRequestMessageBuilder.ConfigureHttpRequestMessage(new HttpMethod("PATCH"), Segment, Credentials);

#else

            var result = HttpRequestMessageBuilder.ConfigureHttpRequestMessage(HttpMethod.Patch, Segment, Credentials);
#endif
            //Assert
            Assert.NotNull(result);
            Assert.IsType<HttpRequestMessage>(result);
#if NETSTANDARD2_0
            Assert.Equal(new HttpMethod("PATCH"), result.Method);
#else
            Assert.Equal(HttpMethod.Patch, result.Method);
#endif


            Assert.Equal(Credentials.ApiKey, result.Headers.Authorization.Parameter);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConfigureHttpRequestMessage_Should_Return_HttpRequestMessage_When_HttpMethod_Is_Put(bool requireSSL)
        {
            //Arrange
            Credentials.RequireSSL = requireSSL;

            //Act
            var result = HttpRequestMessageBuilder.ConfigureHttpRequestMessage(HttpMethod.Put, Segment, Credentials);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<HttpRequestMessage>(result);
            Assert.Equal(HttpMethod.Put, result.Method);
            Assert.Equal(Credentials.ApiKey, result.Headers.Authorization.Parameter);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConfigureHttpRequestMessage_Should_Return_HttpRequestMessage_When_HttpMethod_Is_Delete(bool requireSSL)
        {
            //Arrange
            Credentials.RequireSSL = requireSSL;

            //Act
            var result = HttpRequestMessageBuilder.ConfigureHttpRequestMessage(HttpMethod.Delete, Segment, Credentials);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<HttpRequestMessage>(result);
            Assert.Equal(HttpMethod.Delete, result.Method);
            Assert.Equal(Credentials.ApiKey, result.Headers.Authorization.Parameter);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateHttpRequestMessage_Should_Return_HttpRequestMessage_When_HttpMethod_Is_Get_No_QueryParameters(bool requireSSL)
        {
            //Arrange
            Credentials.RequireSSL = requireSSL;

            //Act
            var result = HttpRequestMessageBuilder.CreateHttpRequestMessage(HttpMethod.Get, Segment, Credentials);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<HttpRequestMessage>(result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateHttpRequestMessage_Should_Return_HttpRequestMessage_When_HttpMethod_Is_Get_With_QueryParameters(bool requireSSL)
        {
            //Arrange
            Credentials.RequireSSL = requireSSL;
            var queryParameters = new LiveTranscriptionOptions()
            {
                Keywords = new[] { "key", "word" }
            };
            //Act
            var result = HttpRequestMessageBuilder.CreateHttpRequestMessage(HttpMethod.Get, Segment, Credentials, null, queryParameters);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<HttpRequestMessage>(result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateHttpRequestMessage_Should_Return_HttpRequestMessage_When_HttpMethod_Is_Post_With_Body(bool requireSSL)
        {
            //Arrange
            Credentials.RequireSSL = requireSSL;
            var urlSource = new UrlSource("https://static.deepgram.com/examples/Bueller-Life-moves-pretty-fast.wav");
            var queryParameters = new PrerecordedTranscriptionOptions()
            {
                Punctuate = true,
                Utterances = true,
                Redaction = new[] { "pci", "ssn" }
            };
            //Act
            var result = HttpRequestMessageBuilder.CreateHttpRequestMessage(HttpMethod.Get, Segment, Credentials, urlSource, queryParameters);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<HttpRequestMessage>(result);
            var content = result.Content;
            Assert.NotNull(result.Content);
            Assert.Equal("application/json", content.Headers.ContentType.MediaType);

        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateHttpRequestMessage_Should_Return_HttpRequestMessage_When_HttpMethod_Is_Put_With_Body(bool requireSSL)
        {
            //Arrange
            Credentials.RequireSSL = requireSSL;
            var body = new UpdateScopeOptions() { Scope = "owner" };
            //Act
            var result = HttpRequestMessageBuilder.CreateHttpRequestMessage(HttpMethod.Get, Segment, Credentials, body, null);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<HttpRequestMessage>(result);
            var content = result.Content;
            Assert.NotNull(result.Content);
            Assert.Equal("application/json", content.Headers.ContentType.MediaType);

        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateHttpRequestMessage_Should_Return_HttpRequestMessage_When_HttpMethod_Is_Patch_With_Body(bool requireSSL)
        {
            //Arrange
            Credentials.RequireSSL = requireSSL;
            var body = new Project()
            {
                Company = "testCompany",
                Id = "testId",
                Name = "test"
            };
            //Act
            var result = HttpRequestMessageBuilder.CreateHttpRequestMessage(HttpMethod.Get, Segment, Credentials, body, null);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<HttpRequestMessage>(result);
            var content = result.Content;
            Assert.NotNull(result.Content);
            Assert.Equal("application/json", content.Headers.ContentType.MediaType);

        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateStreamHttpRequestMessage_Should_Return_HttpRequestMessage_When_HttpMethod_Is_Post_With_Body(bool requireSSL)
        {
            //Arrange
            Credentials.RequireSSL = requireSSL;

            var stream = new MemoryStream(new byte[] { 0b1, 0b10, 0b11, 0b100 });

            var streamSource = new StreamSource(stream, "text/plain");


            //Act
            var result = HttpRequestMessageBuilder.CreateStreamHttpRequestMessage(HttpMethod.Get, Segment, Credentials, streamSource);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<HttpRequestMessage>(result);
            var content = result.Content;
            Assert.NotNull(result.Content);
            Assert.Equal("text/plain", content.Headers.ContentType.MediaType);

        }


    }
}