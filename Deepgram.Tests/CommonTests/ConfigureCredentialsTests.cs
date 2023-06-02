using Deepgram.Common;
using Deepgram.Models;

namespace Deepgram.Tests.CommonTests;
public class ConfigureCredentialsTests
{
    [Fact]
    public void Should_Return_APIKey_If_Key_Present_In_AppSettings()
    {
        //Act
        var fakeKey = Guid.NewGuid().ToString();
        var result = ConfigureCredentials.ConfigureApiKey(new AppSettings() { ApiKey = fakeKey }, null);

        //Assert
        Assert.NotNull(result);
        Assert.IsType<string>(result);
        Assert.Equal(fakeKey, result);
    }

    [Fact]
    public void Should_Return_Same_APIKey_That_Passed_As_Parameter()
    {
        //Act
        var fakeKey = Guid.NewGuid().ToString();
        var result = ConfigureCredentials.ConfigureApiKey(new AppSettings(), fakeKey);

        //Assert
        Assert.NotNull(result);
        Assert.IsType<string>(result);
        Assert.Equal(fakeKey, result);
    }

    [Fact]
    public void Should_Throw_ArgumentException_If_No_ApiKey_Found()
    {
        //Act

        var result = Assert.Throws<ArgumentException>(() => ConfigureCredentials.ConfigureApiKey(new AppSettings(), null));

        //Assert
        Assert.IsType<ArgumentException>(result);
        Assert.Equal(result.Message, "Deepgram API Key must be provided in constructor or via settings");
    }

    [Fact]
    public void Should_Return_TrimmedApiUrl_That_Passed_As_Parameter()
    {
        //Act
        var fakeUrl = "http://test.com";
        var result = ConfigureCredentials.ConfigureApiUrl(new AppSettings(), fakeUrl);

        //Assert
        Assert.NotNull(result);
        Assert.IsType<string>(result);
        Assert.Equal("test.com", result);
    }

    [Fact]
    public void Should_Return_TrimmedApiUrl_If_ApiUrl_Present_In_AppSettings()
    {
        //Act
        var fakeUrl = "http://test.com";
        var result = ConfigureCredentials.ConfigureApiUrl(new AppSettings() { ApiUrl = fakeUrl }, null);

        //Assert
        Assert.NotNull(result);
        Assert.IsType<string>(result);
        Assert.Equal("test.com", result);
    }


    [Fact]
    public void Should_Return_DefaultApiUrl_If_NO_ApiUrl_Present_In_Parameters()
    {
        //Act

        var result = ConfigureCredentials.ConfigureApiUrl(new AppSettings(), null);

        //Assert
        Assert.NotNull(result);
        Assert.IsType<string>(result);
        Assert.Equal("api.deepgram.com", result);
    }


    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    public void Should_Return_RequireSSL_That_Passed_In_AppSettings(string requireSSL)
    {
        //Act

        var result = ConfigureCredentials.ConfigureRequireSSL(new AppSettings() { RequireSSL = requireSSL }, null);

        //Assert

        Assert.IsType<bool>(result);
        Assert.Equal(Convert.ToBoolean(requireSSL), result);

    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Should_Return_RequireSSL_That_Passed_As_Parameter(Nullable<bool> requireSSL)
    {
        //Act

        var result = ConfigureCredentials.ConfigureRequireSSL(new AppSettings(), requireSSL);

        //Assert
        Assert.IsType<bool>(result);
        Assert.Equal(requireSSL, result);
    }

    [Fact]
    public void Should_Return_True_If_No_RequireSSL_Found()
    {
        //Act

        var result = ConfigureCredentials.ConfigureRequireSSL(new AppSettings(), null);

        //Assert
        Assert.IsType<bool>(result);
        Assert.True(result);
    }
}
