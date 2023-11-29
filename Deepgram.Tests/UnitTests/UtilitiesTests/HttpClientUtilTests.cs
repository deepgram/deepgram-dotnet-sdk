namespace Deepgram.Tests.UnitTests.UtilitiesTests;
public class HttpClientUtilTests
{
    string CustomUrl = "acme.com";
    [Test]
    public void Should_Return_HttpClient_With_Default_BaseAddress()
    {
        //Arrange 
        var apiKey = Guid.NewGuid().ToString();
        var clientOptions = new DeepgramClientOptions();
        var httpClient = new HttpClient();
        var expectedBaseAddress = $"{Constants.DEFAULT_URI}/";

        //Act 
        var SUT = HttpClientUtil.Configure(apiKey, clientOptions, httpClient);

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
        var clientOptions = new DeepgramClientOptions();
        var expectedBaseAddress = $"https://{CustomUrl}/";
        clientOptions.BaseAddress = expectedBaseAddress;
        var httpClient = new HttpClient();


        //Act 
        var SUT = HttpClientUtil.Configure(apiKey, clientOptions, httpClient);

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
        var clientOptions = new DeepgramClientOptions();
        var expectedHeaders = FakeHeaders();
        clientOptions.Headers = expectedHeaders;
        var httpClient = new HttpClient();
        var expectedBaseAddress = $"{Constants.DEFAULT_URI}/";

        //Act 
        var SUT = HttpClientUtil.Configure(apiKey, clientOptions, httpClient);

        //Assert 
        Assert.Multiple(() =>
        {
            Assert.That(SUT, Is.Not.Null);
            Assert.That(SUT.BaseAddress, Is.Not.Null);

            Assert.That(SUT.BaseAddress!.ToString(), Is.EqualTo(expectedBaseAddress));

            Assert.That(SUT.DefaultRequestHeaders.Contains(expectedHeaders.First().Key), Is.True);
        });
    }

    [Test]
    public void Should_Return_HttpClient_With_Custom_BaseAddress_And_Custom_Headers()
    {
        //Arrange 
        var apiKey = Guid.NewGuid().ToString();
        var clientOptions = new DeepgramClientOptions();
        var expectedHeaders = FakeHeaders();
        clientOptions.Headers = expectedHeaders;
        var httpClient = new HttpClient();
        var expectedBaseAddress = $"https://{CustomUrl}/";
        clientOptions.BaseAddress = expectedBaseAddress;


        //Act 
        var SUT = HttpClientUtil.Configure(apiKey, clientOptions, httpClient);

        //Assert 
        Assert.Multiple(() =>
        {
            Assert.That(SUT, Is.Not.Null);
            Assert.That(SUT.BaseAddress, Is.Not.Null);

            Assert.That(SUT.BaseAddress!.ToString(), Is.EqualTo(expectedBaseAddress));
            Assert.That(SUT.DefaultRequestHeaders.Contains(expectedHeaders.First().Key), Is.True);

        });
    }


    private static Dictionary<string, string> FakeHeaders()
    {
        var headers = new Dictionary<string, string>();
        var headersCount = new Random().Next(1, 3);
        for (var i = 0; i < headersCount; i++)
        {
            headers.Add($"key{i}", $"value{i}");
        }

        return headers;
    }
}

