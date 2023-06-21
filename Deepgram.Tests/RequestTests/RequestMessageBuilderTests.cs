using Deepgram.Models;
using Deepgram.Request;

namespace Deepgram.Tests.ApiRequestTests;
public class RequestMessageBuilderTests
{
    Credentials Credentials;
    string Segment;
    public RequestMessageBuilderTests()
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
    public void CreateHttpRequestMessage_Should_Return_HttpRequestMessage_When_HttpMethod_Is_Get_No_QueryParameters(bool requireSSL)
    {
        //Arrange
        Credentials.RequireSSL = requireSSL;

        //Act
        var result = RequestMessageBuilder.CreateHttpRequestMessage(HttpMethod.Get, Segment, Credentials);

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
        var queryParameters = new LiveTranscriptionOptions()
        {
            Keywords = new[] { "key", "word" }
        };
        //Act
        var result = RequestMessageBuilder.CreateHttpRequestMessage(HttpMethod.Get, Segment, Credentials, null, queryParameters);

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
        var urlSource = new UrlSource("https://static.deepgram.com/examples/Bueller-Life-moves-pretty-fast.wav");
        var queryParameters = new PrerecordedTranscriptionOptions()
        {
            Punctuate = true,
            Utterances = true,
            Redaction = new[] { "pci", "ssn" }
        };
        //Act
        var result = RequestMessageBuilder.CreateHttpRequestMessage(HttpMethod.Post, Segment, Credentials, urlSource, queryParameters);

        //Assert
        Assert.NotNull(result);
        Assert.IsType<HttpRequestMessage>(result);
        Assert.Equal(HttpMethod.Post, result.Method);
        Assert.Equal(Credentials.ApiKey, result.Headers.Authorization.Parameter);
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
        var result = RequestMessageBuilder.CreateHttpRequestMessage(HttpMethod.Put, Segment, Credentials, body, null);

        //Assert
        Assert.NotNull(result);
        Assert.IsType<HttpRequestMessage>(result);
        Assert.Equal(HttpMethod.Put, result.Method);
        Assert.Equal(Credentials.ApiKey, result.Headers.Authorization.Parameter);
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
        var result = RequestMessageBuilder.CreateHttpRequestMessage(HttpMethod.Patch, Segment, Credentials, body, null);

        //Assert
        Assert.NotNull(result);
        Assert.IsType<HttpRequestMessage>(result);
        Assert.Equal(HttpMethod.Patch, result.Method);
        Assert.Equal(Credentials.ApiKey, result.Headers.Authorization.Parameter);
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
        var result = RequestMessageBuilder.CreateStreamHttpRequestMessage(HttpMethod.Post, Segment, Credentials, streamSource);

        //Assert
        Assert.NotNull(result);
        Assert.IsType<HttpRequestMessage>(result);
        Assert.Equal(HttpMethod.Post, result.Method);
        Assert.Equal(Credentials.ApiKey, result.Headers.Authorization.Parameter);
        var content = result.Content;
        Assert.NotNull(result.Content);
        Assert.Equal("text/plain", content.Headers.ContentType.MediaType);

    }


}
