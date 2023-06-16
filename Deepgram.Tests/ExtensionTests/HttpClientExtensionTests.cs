using Deepgram.Extensions;

namespace Deepgram.Tests.ExtensionTests;
public class HttpClientExtensionTests
{
    [Fact]
    public void GetUserAgent_Should_Return_HttpClient_With_Accept_And_UserAgent_Headers_Set()
    {
        //Arrange 
        var agent = Helpers.UserAgentHelper.GetUserAgent();

        //Act
        var result = HttpClientExtension.Create();


        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<HttpClient>(result);
        Assert.Equal("application/json", result.DefaultRequestHeaders.Accept.ToString());
        Assert.Equal(agent, result.DefaultRequestHeaders.UserAgent.ToString());

    }
}
