namespace Deepgram.Tests;

public class DeepgramClientTests
{
    [Fact]
    public void ProvidingNoAPIKeyThrowsError()
    {
        // Act
        var caughtException = Assert.Throws<ArgumentException>(() => new DeepgramClient(new Credentials()));

        // Assert
        Assert.Equal("Deepgram API Key must be provided in constructor or via settings", caughtException.Message);
    }
}