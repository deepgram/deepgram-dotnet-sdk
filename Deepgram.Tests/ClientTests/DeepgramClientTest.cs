namespace Deepgram.Tests.ClientTests;

public class DeepgramClientTests
{
    [Fact]
    public void Should_Throw_Exception_When_No_ApiKey_Present()
    {
        //Act
        var ex = Assert.Throws<ArgumentException>(() => _ = new DeepgramClient(new Credentials()));

        //Assert
        Assert.IsAssignableFrom<ArgumentException>(ex);
        Assert.Equal("Deepgram API Key must be provided in constructor", ex.Message);
    }


    [Fact]
    public void Should_Initialize_Clients()
    {

        //Act
        var result = new DeepgramClient(new CredentialsFaker().Generate());

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<KeyClient>(result.Keys);
        Assert.IsAssignableFrom<ProjectClient>(result.Projects);
        Assert.IsAssignableFrom<TranscriptionClient>(result.Transcription);
        Assert.IsAssignableFrom<UsageClient>(result.Usage);
    }

    [Fact]
    public void Should_Initialize_LiveTranscriptionClient()
    {
        //Arrange

        var SUT = new DeepgramClient(new CredentialsFaker().Generate());
        //Act

        var result = SUT.CreateLiveTranscriptionClient();

        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<ILiveTranscriptionClient>(result);

    }


    [Theory]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(30)]
    public void Should_Set_TimeOut_On_HttpClient(double timeSpan)
    {

        //Arrange
        var SUT = new DeepgramClient(new CredentialsFaker().Generate());

        //Act
        SUT.SetHttpClientTimeout(TimeSpan.FromSeconds(timeSpan));
        var httpClient = HttpClientUtil.HttpClient;

        //Assert
        Assert.NotNull(httpClient);
        Assert.Equal(timeSpan, httpClient.Timeout.TotalSeconds);
    }


}
