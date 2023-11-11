namespace Deepgram.Tests.UtilitiesTests;
public class HttpConfigureUtilTests
{
    [Test]
    public void Should_Return_HttpClient_With_BaseAddress_Configured()
    {
        //Arrange
        var apiKey = Guid.NewGuid().ToString();
        var clientOptions = new ClientOptionsFaker().Generate("defaults");
        var httpClient = new HttpClient();
        var expectedBaseAddress = $"https://{Constants.DEFAULT_URI}";


        //Act
        var SUT = HttpConfigureUtil.SetBaseUrl(clientOptions.Url!, httpClient);

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(SUT, Is.Not.Null);
            Assert.That(SUT.BaseAddress, Is.Not.Null);
            Assert.That(expectedBaseAddress, Is.EqualTo(SUT.BaseAddress!.ToString()));
        });
    }

    [Test]
    public void Should_Return_HttpClient_With_Default_Headers()
    {
        //Arrange
        var apiKey = Guid.NewGuid().ToString();
        var clientOptions = new ClientOptionsFaker().Generate("defaults");
        clientOptions.Url = "aCustomUri.Test";

        var httpClient = new HttpClient();

        //Act
        var SUT = HttpConfigureUtil.SetBaseUrl(clientOptions.Url, httpClient);

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(SUT, Is.Not.Null);
            Assert.That(SUT.BaseAddress, Is.Not.Null);
            Assert.That(SUT.BaseAddress!.ToString(), Is.EqualTo($"{clientOptions.Url.ToLower()}/"));
        });
    }
}
