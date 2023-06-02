using Deepgram.Models;
using Deepgram.Tests.Fakes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Deepgram.Tests.CommonTests;
public class AppSettingsHelperTests
{
    [Theory]
    [InlineData("1234456789", "http://something.com", "true")]
    [InlineData("1234456789", "http://something.com", "")]
    [InlineData("1234456789", "", "true")]
    [InlineData("1234456789", "", "")]
    public void Should_Return_AppSettings_When_ApiKey_Is_Provided_Success(string? apiKey, string? apiUrl, string? requireSSL)
    {
        //Arrange
        var logMessage = "Authentication provided via configuration";
        var loggerMock = new Mock<ILogger>();

        loggerMock.Setup(l => l.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<IReadOnlyList<KeyValuePair<string, object>>>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<object, Exception, string>>()));


        var SUT = new AppSettingsHelperFake();
        SUT.Configuration = CreateFakeConfiguration(apiKey, apiUrl, requireSSL);
        SUT.CurrentLogger = loggerMock.Object;

        //Act
        var result = SUT.FetchAppSettings();

        //Assert
        Assert.NotNull(result);
        Assert.IsType<AppSettings>(result);
        loggerMock.Verify(l => l.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => ((IReadOnlyList<KeyValuePair<string, object>>)o).Last().Value.ToString().Equals(logMessage)),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once); ;

    }

    [Theory]
    [InlineData("", "http://something.com", "true")]
    [InlineData("", "http://something.com", "")]
    [InlineData("", "", "true")]
    [InlineData("", "", "")]
    public void Should_Return_AppSettings_When_ApiKey_Not_Provided_Success(string? apiKey, string? apiUrl, string? requireSSL)
    {
        //Arrange
        var logMessage = "No authentication found via configuration. Remember to provide your own.";
        var loggerMock = new Mock<ILogger>();

        loggerMock.Setup(l => l.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<IReadOnlyList<KeyValuePair<string, object>>>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<object, Exception, string>>()));


        var SUT = new AppSettingsHelperFake();
        SUT.Configuration = CreateFakeConfiguration(apiKey, apiUrl, requireSSL);
        SUT.CurrentLogger = loggerMock.Object;

        //Act
        var result = SUT.FetchAppSettings();

        //Assert
        Assert.NotNull(result);
        Assert.IsType<AppSettings>(result);
        Assert.Equal(apiKey, result.ApiKey);
        loggerMock.Verify(l => l.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => ((IReadOnlyList<KeyValuePair<string, object>>)o).Last().Value.ToString().Equals(logMessage)),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);

    }

    [Fact]
    public void Should_Get_RequestsPerSecond_From_Json_Files()
    {
        var SUT = new AppSettingsHelperFake();
        SUT.Configuration = CreateFakeConfiguration("fake", "", "");


        //Act
        var result = SUT.GetRequestsPerSecond();

        //Assert
        Assert.NotNull(result);
        Assert.IsType<double>(result);

    }


    private IConfigurationRoot CreateFakeConfiguration(string apiKey, string? apiUrl, string? requireSSL)
    {
        var inMemorySettings = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(apiKey))
            inMemorySettings.Add("appSettings:Deepgram.Api.Key", $"{apiKey}");

        if (!string.IsNullOrEmpty(apiUrl))
            inMemorySettings.Add("appSettings:Deepgram.Api.Uri", $"{apiUrl}");

        if (!string.IsNullOrEmpty(requireSSL))
            inMemorySettings.Add("appSettings:Deepgram.Api.RequireSSL", $"{requireSSL}");



        return new ConfigurationBuilder()
             .AddInMemoryCollection(inMemorySettings)
             .Build();

    }
}
