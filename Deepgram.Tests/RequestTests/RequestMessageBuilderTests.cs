using System.Net.Http;
using AutoBogus;
using Bogus;
using Deepgram.Models;
using Deepgram.Request;
using Deepgram.Tests.Fakers;
using Xunit;

namespace Deepgram.Tests.RequestTests
{
    public class RequestMessageBuilderTests
    {
        Credentials _credentials;
        RequestMessageBuilder SUT;
        string _uriSegment;
        UrlSource _urlSource;
        PrerecordedTranscriptionOptions _prerecordedTranscriptionOptions;

        public RequestMessageBuilderTests()
        {
            _credentials = new CredentialsFaker().Generate();
            SUT = new RequestMessageBuilder();
            _uriSegment = new Faker().Lorem.Word();
            _urlSource = new UrlSource(new Faker().Internet.Url());
            _prerecordedTranscriptionOptions = new PrerecordedTranscriptionOptionsFaker().Generate();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateHttpRequestMessage_Should_Return_HttpRequestMessage_When_HttpMethod_Is_Get_No_QueryParameters(bool requireSSL)
        {
            //Arrange            
            _credentials.RequireSSL = requireSSL;
            var SUT = new RequestMessageBuilder();
            //Act
            var result = SUT.CreateHttpRequestMessage(HttpMethod.Get, _uriSegment, _credentials);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<HttpRequestMessage>(result);
            Assert.Equal(HttpMethod.Get, result.Method);
            Assert.Equal(_credentials.ApiKey, result.Headers.Authorization.Parameter);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateHttpRequestMessage_Should_Return_HttpRequestMessage_When_HttpMethod_Is_Get_With_QueryParameters(bool requireSSL)
        {
            //Arrange
            _credentials.RequireSSL = requireSSL;

            //Act
            var result = SUT.CreateHttpRequestMessage(HttpMethod.Get, _uriSegment, _credentials, null, _prerecordedTranscriptionOptions);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<HttpRequestMessage>(result);
            Assert.Equal(HttpMethod.Get, result.Method);
            Assert.Equal(_credentials.ApiKey, result.Headers.Authorization.Parameter);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateHttpRequestMessage_Should_Return_HttpRequestMessage_When_HttpMethod_Is_Post_With_Body(bool requireSSL)
        {
            //Arrange
            _credentials.RequireSSL = requireSSL;


            //Act
            var result = SUT.CreateHttpRequestMessage(HttpMethod.Post, _uriSegment, _credentials, _urlSource, _prerecordedTranscriptionOptions);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<HttpRequestMessage>(result);
            var content = result.Content;
            Assert.NotNull(result.Content);
            Assert.Equal(HttpMethod.Post, result.Method);
            Assert.Equal(_credentials.ApiKey, result.Headers.Authorization.Parameter);
            Assert.Equal("application/json", content.Headers.ContentType.MediaType);

        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateHttpRequestMessage_Should_Return_HttpRequestMessage_When_HttpMethod_Is_Put_With_Body(bool requireSSL)
        {
            //Arrange
            _credentials.RequireSSL = requireSSL;
            var options = new AutoFaker<UpdateScopeOptions>().Generate();
            //Act
            var result = SUT.CreateHttpRequestMessage(HttpMethod.Put, _uriSegment, _credentials, options, null);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<HttpRequestMessage>(result);
            var content = result.Content;
            Assert.NotNull(result.Content);
            Assert.Equal(HttpMethod.Put, result.Method);
            Assert.Equal(_credentials.ApiKey, result.Headers.Authorization.Parameter);
            Assert.Equal("application/json", content.Headers.ContentType.MediaType);

        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateHttpRequestMessage_Should_Return_HttpRequestMessage_When_HttpMethod_Is_Patch_With_Body(bool requireSSL)
        {
            //Arrange
            _credentials.RequireSSL = requireSSL;
            var project = new AutoFaker<Project>().Generate();

            //Act

#if NETSTANDARD2_0
            var result = SUT.CreateHttpRequestMessage(new HttpMethod("PATCH"), _uriSegment, _credentials, project, null);
#else            
            var result = SUT.CreateHttpRequestMessage(HttpMethod.Patch, _uriSegment, _credentials, project, null);
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
            Assert.Equal(_credentials.ApiKey, result.Headers.Authorization.Parameter);
            Assert.Equal("application/json", content.Headers.ContentType.MediaType);

        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateStreamHttpRequestMessage_Should_Return_HttpRequestMessage_When_HttpMethod_Is_Post_With_Body(bool requireSSL)
        {
            //Arrange
            _credentials.RequireSSL = requireSSL;
            var streamSource = new StreamSourceFaker().Generate();
            //Act
            var result = SUT.CreateStreamHttpRequestMessage(HttpMethod.Post, _uriSegment, _credentials, streamSource);

            //Assert
            Assert.NotNull(result);
            Assert.IsType<HttpRequestMessage>(result);
            var content = result.Content;
            Assert.NotNull(result.Content);
            Assert.Equal(HttpMethod.Post, result.Method);
            Assert.Equal(_credentials.ApiKey, result.Headers.Authorization.Parameter);
            Assert.Equal(streamSource.MimeType, content.Headers.ContentType.MediaType);

        }


    }
}