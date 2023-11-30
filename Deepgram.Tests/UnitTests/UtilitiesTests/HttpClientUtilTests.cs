namespace Deepgram.Tests.UnitTests.UtilitiesTests;
public class HttpClientUtilTests
{
    readonly string _customUrl = "acme.com";
    [Test]
    public void Should_Return_HttpClient_With_Default_BaseAddress()
    {
        //Arrange 
        var apiKey = Guid.NewGuid().ToString();
        var clientOptions = new DeepgramClientOptions();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(new MessageResponse(), HttpStatusCode.OK);

        httpClientFactory.CreateClient(Constants.HTTPCLIENT_NAME).Returns(httpClient);
        var expectedBaseAddress = $"{Constants.DEFAULT_URI}/";

        //Act 
        var SUT = HttpClientUtil.Configure(apiKey, clientOptions, httpClientFactory);

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
        var expectedBaseAddress = $"https://{_customUrl}/";
        clientOptions.BaseAddress = expectedBaseAddress;
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(new MessageResponse(), HttpStatusCode.OK);

        httpClientFactory.CreateClient(Constants.HTTPCLIENT_NAME).Returns(httpClient);



        //Act 
        var SUT = HttpClientUtil.Configure(apiKey, clientOptions, httpClientFactory);

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
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(new MessageResponse(), HttpStatusCode.OK);
        httpClientFactory.CreateClient(Constants.HTTPCLIENT_NAME).Returns(httpClient);

        var expectedBaseAddress = $"{Constants.DEFAULT_URI}/";

        //Act 
        var SUT = HttpClientUtil.Configure(apiKey, clientOptions, httpClientFactory);

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
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(new MessageResponse(), HttpStatusCode.OK);

        httpClientFactory.CreateClient(Constants.HTTPCLIENT_NAME).Returns(httpClient);

        var expectedBaseAddress = $"https://{_customUrl}/";
        clientOptions.BaseAddress = expectedBaseAddress;


        //Act 
        var SUT = HttpClientUtil.Configure(apiKey, clientOptions, httpClientFactory);

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

