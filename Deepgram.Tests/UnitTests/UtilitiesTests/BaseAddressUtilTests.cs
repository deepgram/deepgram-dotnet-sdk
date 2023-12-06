namespace Deepgram.Tests.UnitTests.UtilitiesTests;
public class BaseAddressUtilTests
{
    DeepgramClientOptions _deepgramClientOptions;
    string _customAddressPart;

    [SetUp]
    public void SetUp()
    {
        _deepgramClientOptions = new DeepgramClientOptions();
        _customAddressPart = "acme.com";
    }

    [Test]
    public void GetHTTPS_Should_Return_DefaultAddress_When_No_BaseAddress_Explicitly_Set()
    {
        //Act
        var result = BaseAddressUtil.GetHttps(_deepgramClientOptions);

        //Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.BaseAddress.Should().NotBeNullOrEmpty();
            result.BaseAddress.Should().BeEquivalentTo($"https://{Constants.DEFAULT_URI}");
        }
    }

    [Test]
    public void GetHTTPS_Should_Return_DefaultAddress_When_No_BaseAddress_with_HTTP_protocol()
    {
        //Arrange        
        _deepgramClientOptions.BaseAddress = $"http://{_customAddressPart}";


        //Act
        var result = BaseAddressUtil.GetHttps(_deepgramClientOptions);

        //Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.BaseAddress.Should().NotBeNullOrEmpty();
            result.BaseAddress.Should().BeEquivalentTo($"https://{_customAddressPart}");
        }
    }
    [Test]
    public void GetHTTPS_Should_Return_DefaultAddress_When_No_BaseAddress_with_HTTPS_protocol()
    {
        //Arrange        
        _deepgramClientOptions.BaseAddress = $"https://{_customAddressPart}";


        //Act
        var result = BaseAddressUtil.GetHttps(_deepgramClientOptions);

        //Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.BaseAddress.Should().NotBeNullOrEmpty();
            result.BaseAddress.Should().BeEquivalentTo($"https://{_customAddressPart}");
        }
    }

    [Test]
    public void GetWSS_Should_Return_DefaultAddress_When_No_BaseAddress_Explicitly_Set()
    {
        //Act
        var result = BaseAddressUtil.GetWss(_deepgramClientOptions);

        //Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.BaseAddress.Should().NotBeNullOrEmpty();
            result.BaseAddress.Should().BeEquivalentTo($"wss://{Constants.DEFAULT_URI}");
        }
    }

    [Test]
    public void GetWSS_Should_Return_DefaultAddress_When_No_BaseAddress_with_WS_protocol()
    {
        //Arrange        
        _deepgramClientOptions.BaseAddress = $"ws://{_customAddressPart}";


        //Act
        var result = BaseAddressUtil.GetWss(_deepgramClientOptions);

        //Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.BaseAddress.Should().NotBeNullOrEmpty();
            result.BaseAddress.Should().BeEquivalentTo($"wss://{_customAddressPart}");
        }
    }
    [Test]
    public void GetWSS_Should_Return_DefaultAddress_When_No_BaseAddress_with_WSS_protocol()
    {
        //Arrange        
        _deepgramClientOptions.BaseAddress = $"wss://{_customAddressPart}";


        //Act
        var result = BaseAddressUtil.GetWss(_deepgramClientOptions);

        //Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.BaseAddress.Should().NotBeNullOrEmpty();
            result.BaseAddress.Should().BeEquivalentTo($"wss://{_customAddressPart}");
        }
    }

}
