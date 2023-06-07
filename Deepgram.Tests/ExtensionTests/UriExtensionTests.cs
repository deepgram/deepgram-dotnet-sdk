using Deepgram.Extensions;
using Deepgram.Models;

namespace Deepgram.Tests.ExtensionTests;
public class UriExtensionTests
{
    public string fakeUriSegment;
    public Credentials credentials;

    public UriExtensionTests()
    {
        credentials = new Credentials() { ApiKey = string.Empty, ApiUrl = "fake.com", RequireSSL = null };
        fakeUriSegment = "/projects/test";
    }

    [Theory]
    [InlineData("wss")]
    [InlineData("ws")]
    [InlineData("https")]
    [InlineData("http")]
    public void Should_Return_URI_With_No_Query_Parameters(string protocol)
    {
        //Act 
        var result = UriExtension.ResolveUri(credentials, fakeUriSegment, protocol, null);

        //
        Assert.NotNull(result);
        Assert.IsType<Uri>(result);
        Assert.Equal(protocol, result.Scheme);
        Assert.Equal($"{protocol}://{credentials.ApiUrl}/v1{fakeUriSegment}", result.AbsoluteUri);
    }



    //only tested a few types of options as not a check for valid
    //parameter conversion - checking for correctly formatted uri
    [Theory]
    [InlineData("wss")]
    [InlineData("ws")]
    [InlineData("https")]
    [InlineData("http")]
    public void Should_Return_URI_With_LiveTranscriptionOptions_Query_Parameters(string protocol)
    {

        //Arrange
        var fakeParameter = "fake";
        var queryParameters = new LiveTranscriptionOptions()
        {
            Keywords = new string[] { fakeParameter }
        };


        //Act 
        var result = UriExtension.ResolveUri(credentials, fakeUriSegment, protocol, queryParameters);

        //
        Assert.NotNull(result);
        Assert.IsType<Uri>(result);
        Assert.Equal(protocol, result.Scheme);
        Assert.Equal($"{protocol}://{credentials.ApiUrl}/v1{fakeUriSegment}?{nameof(LiveTranscriptionOptions.Keywords).ToLower()}={fakeParameter}", result.AbsoluteUri);
    }

    [Theory]
    [InlineData("wss")]
    [InlineData("ws")]
    [InlineData("https")]
    [InlineData("http")]
    public void Should_Return_URI_With_UpdateScopeOptions_Query_Parameters(string protocol)
    {

        //Arrange
        var fakeParameter = "fake";
        var queryParameters = new UpdateScopeOptions()
        {
            Scope = fakeParameter
        };


        //Act 
        var result = UriExtension.ResolveUri(credentials, fakeUriSegment, protocol, queryParameters);


        Assert.NotNull(result);
        Assert.IsType<Uri>(result);
        Assert.Equal(protocol, result.Scheme);
        Assert.Equal($"{protocol}://{credentials.ApiUrl}/v1{fakeUriSegment}?{nameof(UpdateScopeOptions.Scope).ToLower()}={fakeParameter}", result.AbsoluteUri);

    }
}
