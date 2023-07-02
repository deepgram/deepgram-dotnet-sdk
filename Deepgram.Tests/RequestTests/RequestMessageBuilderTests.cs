namespace Deepgram.Tests.RequestTests;

public class RequestMessageBuilderTests
{
    readonly CleanCredentials _credentials;
    readonly RequestMessageBuilder _sUT;
    readonly string _uriSegment;
    readonly UrlSource _urlSource;
    readonly PrerecordedTranscriptionOptions _prerecordedTranscriptionOptions;

    public RequestMessageBuilderTests()
    {
        _credentials = new CleanCredentialsFaker().Generate();
        _sUT = new RequestMessageBuilder();
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
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        Assert.Equal(_credentials.ApiKey, result.Headers.Authorization.Parameter);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CreateHttpRequestMessage_Should_Return_HttpRequestMessage_When_HttpMethod_Is_Get_With_QueryParameters(bool requireSSL)
    {
        //Arrange
        _credentials.RequireSSL = requireSSL;

        //Act
        var result = _sUT.CreateHttpRequestMessage(HttpMethod.Get, _uriSegment, _credentials, null, _prerecordedTranscriptionOptions);

        //Assert
        Assert.NotNull(result);
        Assert.IsType<HttpRequestMessage>(result);
        Assert.Equal(HttpMethod.Get, result.Method);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        Assert.Equal(_credentials.ApiKey, result.Headers.Authorization.Parameter);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CreateHttpRequestMessage_Should_Return_HttpRequestMessage_When_HttpMethod_Is_Post_With_Body(bool requireSSL)
    {
        //Arrange
        _credentials.RequireSSL = requireSSL;


        //Act
        var result = _sUT.CreateHttpRequestMessage(HttpMethod.Post, _uriSegment, _credentials, _urlSource, _prerecordedTranscriptionOptions);

        //Assert
        Assert.NotNull(result);
        Assert.IsType<HttpRequestMessage>(result);
        var content = result.Content;
        Assert.NotNull(result.Content);
        Assert.Equal(HttpMethod.Post, result.Method);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        Assert.Equal(_credentials.ApiKey, result.Headers.Authorization.Parameter);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        Assert.Equal("application/json", content.Headers.ContentType.MediaType!);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

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
        var result = _sUT.CreateHttpRequestMessage(HttpMethod.Put, _uriSegment, _credentials, options, null);

        //Assert
        Assert.NotNull(result);
        Assert.IsType<HttpRequestMessage>(result);
        var content = result.Content;
        Assert.NotNull(result.Content);
        Assert.Equal(HttpMethod.Put, result.Method);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        Assert.Equal(_credentials.ApiKey, result.Headers.Authorization.Parameter);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        Assert.Equal("application/json", content.Headers.ContentType.MediaType);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

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
        var result = _sUT.CreateHttpRequestMessage(HttpMethod.Patch, _uriSegment, _credentials, project, null);

        //Assert
        Assert.NotNull(result);
        Assert.IsType<HttpRequestMessage>(result);
        var content = result.Content;
        Assert.NotNull(result.Content);
        Assert.Equal(HttpMethod.Patch, result.Method);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        Assert.Equal(_credentials.ApiKey, result.Headers.Authorization.Parameter);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        Assert.Equal("application/json", content.Headers.ContentType.MediaType);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
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
        var result = _sUT.CreateStreamHttpRequestMessage(HttpMethod.Post, _uriSegment, _credentials, streamSource);

        //Assert
        Assert.NotNull(result);
        Assert.IsType<HttpRequestMessage>(result);
        var content = result.Content!;
        Assert.NotNull(result.Content);
        Assert.Equal(HttpMethod.Post, result.Method);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        Assert.Equal(_credentials.ApiKey, result.Headers.Authorization.Parameter);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        Assert.Equal(streamSource.MimeType, content.Headers.ContentType.MediaType);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

    }


}