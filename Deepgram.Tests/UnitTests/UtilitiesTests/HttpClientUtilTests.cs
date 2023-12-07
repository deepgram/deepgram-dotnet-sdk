using Deepgram.Extensions;
using Deepgram.Models;
using Deepgram.Records;

namespace Deepgram.Tests.UnitTests.UtilitiesTests;
public class HttpClientUtilTests
{
    readonly string _customUrl = "acme.com";
    IHttpClientFactory _httpClientFactory;
    string _apiKey;
    DeepgramClientOptions _clientOptions;

    [SetUp]
    public void Setup()
    {
        _apiKey = Guid.NewGuid().ToString();
        _httpClientFactory = Substitute.For<IHttpClientFactory>();
        _clientOptions = new DeepgramClientOptions("fakeKey");
    }


    [Test]
    public void Should_Return_HttpClient_With_Default_BaseAddress()
    {
        //Arrange 
        var httpClient = MockHttpClient.CreateHttpClientWithResult(new MessageResponse(), HttpStatusCode.OK);
        _httpClientFactory.CreateClient(Constants.HTTPCLIENT_NAME).Returns(httpClient);

        //Act
        var SUT = httpClient.ConfigureDeepgram(_clientOptions);

        //Assert 
        using (new AssertionScope())
        {
            SUT.Should().NotBeNull();
            SUT.BaseAddress.Should().Be($"https://{Constants.DEFAULT_URI}/");
        };
    }

    [Test]
    public void Should_Return_HttpClient_With_Custom_BaseAddress()
    {
        //Arrange        
        var expectedBaseAddress = $"https://{_customUrl}/";
        _clientOptions.BaseAddress = expectedBaseAddress;
        var httpClient = MockHttpClient.CreateHttpClientWithResult(new MessageResponse(), HttpStatusCode.OK, expectedBaseAddress);
        _httpClientFactory.CreateClient(Constants.HTTPCLIENT_NAME).Returns(httpClient);

        //Act
        var SUT = httpClient.ConfigureDeepgram(_clientOptions);

        //Assert 
        using (new AssertionScope())
        {
            SUT.Should().NotBeNull();
            SUT.BaseAddress.Should().Be(expectedBaseAddress);
        };
    }

    [Test]
    public void Should_Return_HttpClient_With_Default_BaseAddress_And_Custom_Headers()
    {
        //Arrange 
        _clientOptions.Headers = FakeHeaders();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(new MessageResponse(), HttpStatusCode.OK);
        _httpClientFactory.CreateClient(Constants.HTTPCLIENT_NAME).Returns(httpClient);

        //Act
        var SUT = httpClient.ConfigureDeepgram(_clientOptions);

        //Assert 
        using (new AssertionScope())
        {
            SUT.Should().NotBeNull();
            SUT.BaseAddress.Should().Be($"https://{Constants.DEFAULT_URI}/");
            SUT.DefaultRequestHeaders.Should().ContainKey(_clientOptions.Headers.First().Key);
        };
    }

    [Test]
    public void Should_Return_HttpClient_With_Custom_BaseAddress_And_Custom_Headers()
    {
        //Arrange       
        _clientOptions.Headers = FakeHeaders();
        var httpClient = MockHttpClient.CreateHttpClientWithResult(new MessageResponse(), HttpStatusCode.OK);
        _httpClientFactory.CreateClient(Constants.HTTPCLIENT_NAME).Returns(httpClient);
        var expectedBaseAddress = $"https://{_customUrl}/";
        _clientOptions.BaseAddress = expectedBaseAddress;

        //Act
        var SUT = httpClient.ConfigureDeepgram(_clientOptions);

        //Assert 
        using (new AssertionScope())
        {
            SUT.Should().NotBeNull();
            SUT.BaseAddress.Should().Be(expectedBaseAddress);
            SUT.DefaultRequestHeaders.Should().ContainKey(_clientOptions.Headers.First().Key);
        };
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

