using Deepgram.Common;
using Deepgram.Extensions;

namespace Deepgram.Tests.ExtensionTests;
public class HttpClientExtensionTests
{
    [Fact]
    public void Should_Return_HttpClient_With_Accept_And_UserAgent_Headers_Set()
    {
        //Arrange 
        var agent = UserAgentHelper.GetUserAgent();

        //Act
        var sut = HttpClientExtension.Create();


        //Assert
        Assert.NotNull(sut);
        Assert.IsAssignableFrom<HttpClient>(sut);
        Assert.Equal("application/json", sut.DefaultRequestHeaders.Accept.ToString());
        Assert.Equal(agent, sut.DefaultRequestHeaders.UserAgent.ToString());

    }
}
