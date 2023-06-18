using Deepgram.Utillities;

namespace Deepgram.Tests.ExtensionTests;
public class UriUtilTests
{
    [Theory]
    [InlineData("https")]
    [InlineData("http")]
    [InlineData("ws")]
    [InlineData("wss")]

    public void ResolveUri_Should_Return_Uri_Without_Parameters(string protocol)
    {
        //Arrange
        var apiUrl = "test.com";
        var uriSegment = "segment";

        //Act
        var result = UriUtil.ResolveUri(apiUrl, uriSegment, protocol);

        //Assert
        Assert.NotNull(result);
        Assert.IsType<Uri>(result);
        Assert.Equal($"{protocol}://{apiUrl}/v1/{uriSegment}", result.AbsoluteUri);
        Assert.Equal(protocol, result.Scheme);
        Assert.Equal(apiUrl, result.Host);
        Assert.Contains(uriSegment, result.Segments);
    }

    [Theory]
    [InlineData("https")]
    [InlineData("http")]
    [InlineData("ws")]
    [InlineData("wss")]

    public void ResolveUri_Should_Return_Uri_With_Parameters(string protocol)
    {
        //Arrange
        var apiUrl = "test.com";
        var uriSegment = "segment";
        var parameters = new Dictionary<string, string>
        {
            { "key", "value" }
        };
        //Act
        var result = UriUtil.ResolveUri(apiUrl, uriSegment, protocol, parameters);

        //Assert
        Assert.NotNull(result);
        Assert.IsType<Uri>(result);
        Assert.Equal($"{protocol}://{apiUrl}/v1/{uriSegment}?key=value", result.AbsoluteUri);
        Assert.Equal(protocol, result.Scheme);
        Assert.Equal(apiUrl, result.Host);
        Assert.Contains(uriSegment, result.Segments);
        Assert.Contains("key=value", result.Query);
    }
}
