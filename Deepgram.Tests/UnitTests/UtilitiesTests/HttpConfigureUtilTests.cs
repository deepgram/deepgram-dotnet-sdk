namespace Deepgram.Tests.UnitTests.UtilitiesTests;
public class HttpConfigureUtilTests
{
    [Test]
    public void Should_Return_HttpClient_With_Default_BaseAddress()
    {
        //Arrange
        var apiKey = Guid.NewGuid().ToString();
        var clientOptions = new ClientOptionsFaker().Generate("defaults");
        var httpClient = new HttpClient();
        var expectedBaseAddress = $"https://{Constants.DEFAULT_URI}/{Constants.API_VERSION}";

        //Act
        var SUT = HttpConfigureUtil.Configure(apiKey, clientOptions, httpClient);

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(SUT, Is.Not.Null);
            Assert.That(SUT.BaseAddress, Is.Not.Null);
            Assert.That(SUT.BaseAddress!.ToString(), Is.EqualTo(expectedBaseAddress));
        });
    }

    [Test]
    public void Should_Return_HttpClient_With_Custom_BaseAddress()
    {
        //Arrange
        var apiKey = Guid.NewGuid().ToString();
        var clientOptions = new ClientOptionsFaker().Generate("defaults");
        clientOptions.Url = $"acustomuri.test";
        var httpClient = new HttpClient();
        var expectedBaseAddress = $"https://{clientOptions.Url}/";

        //Act
        var SUT = HttpConfigureUtil.Configure(apiKey, clientOptions, httpClient);

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(SUT, Is.Not.Null);
            Assert.That(SUT.BaseAddress, Is.Not.Null);
            Assert.That(SUT.BaseAddress!.ToString(), Is.EqualTo(expectedBaseAddress));
        });
    }

    [Test]
    public void Should_Return_HttpClient_With_Custom_BaseAddress_With_Http_Protocol()
    {
        //Arrange
        var apiKey = Guid.NewGuid().ToString();
        var clientOptions = new ClientOptionsFaker().Generate("defaults");
        var customUrl = "acustomuri.test";
        clientOptions.Url = $"http://{customUrl}";
        var httpClient = new HttpClient();
        var expectedBaseAddress = $"https://{customUrl}/";

        //Act
        var SUT = HttpConfigureUtil.Configure(apiKey, clientOptions, httpClient);

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(SUT, Is.Not.Null);
            Assert.That(SUT.BaseAddress, Is.Not.Null);
            Assert.That(SUT.BaseAddress!.ToString(), Is.EqualTo(expectedBaseAddress));
        });
    }

    [Test]
    public void Should_Return_HttpClient_With_Custom_BaseAddress_With_Https_Protocol()
    {
        //Arrange
        var apiKey = Guid.NewGuid().ToString();
        var clientOptions = new ClientOptionsFaker().Generate("defaults");
        var customUrl = "acustomuri.test";
        clientOptions.Url = $"https://{customUrl}";
        var httpClient = new HttpClient();
        var expectedBaseAddress = $"https://{customUrl}/";

        //Act
        var SUT = HttpConfigureUtil.Configure(apiKey, clientOptions, httpClient);

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(SUT, Is.Not.Null);
            Assert.That(SUT.BaseAddress, Is.Not.Null);
            Assert.That(SUT.BaseAddress!.ToString(), Is.EqualTo(expectedBaseAddress));
        });
    }

    [Test]
    public void Should_Return_HttpClient_With_Default_BaseAddress_And_Custom_Headers()
    {
        //Arrange
        var apiKey = Guid.NewGuid().ToString();
        var clientOptions = new ClientOptionsFaker().Generate("defaults");
        var expectedHeaders = FakeHeaders();
        clientOptions.Headers = expectedHeaders;
        var httpClient = new HttpClient();
        var expectedBaseAddress = $"https://{Constants.DEFAULT_URI}/{Constants.API_VERSION}";

        //Act
        var SUT = HttpConfigureUtil.Configure(apiKey, clientOptions, httpClient);

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(SUT, Is.Not.Null);
            Assert.That(SUT.BaseAddress, Is.Not.Null);

            Assert.That(SUT.BaseAddress!.ToString(), Is.EqualTo(expectedBaseAddress));
            Assert.IsTrue(SUT.DefaultRequestHeaders.Contains(expectedHeaders.First().Key));
        });
    }

    [Test]
    public void Should_Return_HttpClient_With_Custom_BaseAddress_And_Custom_Headers()
    {
        //Arrange
        var apiKey = Guid.NewGuid().ToString();
        var clientOptions = new ClientOptionsFaker().Generate("defaults");
        var expectedHeaders = FakeHeaders();
        clientOptions.Headers = expectedHeaders;
        var httpClient = new HttpClient();
        clientOptions.Url = "acustomuri.test";
        var expectedBaseAddress = $"https://{clientOptions.Url}/";

        //Act
        var SUT = HttpConfigureUtil.Configure(apiKey, clientOptions, httpClient);

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(SUT, Is.Not.Null);
            Assert.That(SUT.BaseAddress, Is.Not.Null);

            Assert.That(SUT.BaseAddress!.ToString(), Is.EqualTo(expectedBaseAddress));
            Assert.IsTrue(SUT.DefaultRequestHeaders.Contains(expectedHeaders.First().Key));
        });
    }

    [Test]
    public void Should_Return_HttpClient_With_PreConfigured_BaseAddress()
    {
        //Arrange
        var apiKey = Guid.NewGuid().ToString();
        var clientOptions = new ClientOptionsFaker().Generate("defaults");
        var expectedBaseAddress = new Uri("https://preconfig.com");
        var httpClient = new HttpClient();
        httpClient.BaseAddress = expectedBaseAddress;


        //Act
        var SUT = HttpConfigureUtil.Configure(apiKey, clientOptions, httpClient);

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(SUT, Is.Not.Null);
            Assert.That(SUT.BaseAddress, Is.Not.Null);

            Assert.That(SUT.BaseAddress!.ToString(), Is.EqualTo(expectedBaseAddress.ToString()));
        });
    }

    [Test]
    public void Should_Return_HttpClient_With_PreConfigured_BaseAddress_And_Custom_Headers()
    {
        //Arrange
        var apiKey = Guid.NewGuid().ToString();
        var clientOptions = new ClientOptionsFaker().Generate("defaults");
        var expectedHeaders = FakeHeaders();
        clientOptions.Headers = expectedHeaders;
        var expectedBaseAddress = new Uri("https://preconfig.com");
        var httpClient = new HttpClient();
        httpClient.BaseAddress = expectedBaseAddress;

        //Act
        var SUT = HttpConfigureUtil.Configure(apiKey, clientOptions, httpClient);

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(SUT, Is.Not.Null);
            Assert.That(SUT.BaseAddress, Is.Not.Null);

            Assert.That(SUT.BaseAddress!.ToString(), Is.EqualTo(expectedBaseAddress.ToString()));
            Assert.IsTrue(SUT.DefaultRequestHeaders.Contains(expectedHeaders.First().Key));
        });
    }


    private static Dictionary<string, string> FakeHeaders()
    {
        var faker = new Faker();
        var headers = new Dictionary<string, string>();
        var headersCount = new Random().Next(1, 3);
        for (var i = 0; i < headersCount; i++)
        {
            headers.Add($"key{i}", $"value{i}");
        }

        return headers;
    }
}
