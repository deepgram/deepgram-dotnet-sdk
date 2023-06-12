using Deepgram.Extensions;

namespace Deepgram.Tests.ExtensionTests;
public class UriExtensionTests
{
    [Theory]
    [InlineData("https")]
    [InlineData("http")]
    [InlineData("ws")]
    [InlineData("wss")]

    public void Should_Return_Uri_Without_Parameters(string protocol)
    {
        //Arrange
        var apiUrl = "test.com";
        var uriSegment = "segment";

        //Act
        var sut = UriExtension.ResolveUri(apiUrl, uriSegment, protocol);

        //Assert
        Assert.NotNull(sut);
        Assert.IsType<Uri>(sut);
        Assert.Equal($"{protocol}://{apiUrl}/v1/{uriSegment}", sut.AbsoluteUri);
        Assert.Equal(protocol, sut.Scheme);
        Assert.Equal(apiUrl, sut.Host);
        Assert.Contains(uriSegment, sut.Segments);
    }

    [Theory]
    [InlineData("https")]
    [InlineData("http")]
    [InlineData("ws")]
    [InlineData("wss")]

    public void Should_Return_Uri_With_Parameters(string protocol)
    {
        //Arrange
        var apiUrl = "test.com";
        var uriSegment = "segment";
        var parameters = new Dictionary<string, string>
        {
            { "key", "value" }
        };
        //Act
        var sut = UriExtension.ResolveUri(apiUrl, uriSegment, protocol, parameters);

        //Assert
        Assert.NotNull(sut);
        Assert.IsType<Uri>(sut);
        Assert.Equal($"{protocol}://{apiUrl}/v1/{uriSegment}?key=value", sut.AbsoluteUri);
        Assert.Equal(protocol, sut.Scheme);
        Assert.Equal(apiUrl, sut.Host);
        Assert.Contains(uriSegment, sut.Segments);
        Assert.Contains("key=value", sut.Query);
    }
}
