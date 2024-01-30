using System.Net.WebSockets;

namespace Deepgram.Tests.UnitTests.ClientTests;
public class LiveClientTests
{
    DeepgramClientOptions _options;
    WebSocketReceiveResult _webSocketReceiveResult;
    LiveClient _liveClient;

    [SetUp]
    public void Setup()
    {
        var apiKey = new Faker().Random.Guid().ToString();
        // will set up with base address set to - api.deepgram.com
        _options = new DeepgramClientOptions();
        _webSocketReceiveResult = new WebSocketReceiveResult(1, WebSocketMessageType.Text, true);
        _liveClient = new LiveClient(apiKey, _options);
    }

    [TearDown]
    public void Teardown()
    { _liveClient.Dispose(); }

    //[Test]
    //public void ProcessDataReceived_Should_Raise_TranscriptReceived_Event_When_Response_Contains_Type_LiveTranscriptionResponse()
    //{
    //    //Arrange
    //    var liveTranscriptionResponse = new AutoFaker<LiveTranscriptionResponse>().Generate();
    //    // ensure the right type is set for testing
    //    liveTranscriptionResponse.Type = Enums.LiveType.Results;
    //    var json = JsonSerializer.Serialize(liveTranscriptionResponse);
    //    var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json));
    //    TranscriptReceivedEventArgs? eventArgs = null;

    //    _liveClient.TranscriptReceived += (sender, args) => eventArgs = args;


    //    //Act
    //    _liveClient.ProcessDataReceived(_webSocketReceiveResult, memoryStream);
    //    Task.Delay(5000);
    //    //Assert
    //    using (new AssertionScope())
    //    {
    //        eventArgs.Should().NotBeNull();
    //        eventArgs!.Transcript.Should().NotBeNull();
    //        eventArgs.Transcript.Should().BeAssignableTo<LiveTranscriptionResponse>();
    //        eventArgs.Transcript.Should().BeEquivalentTo(liveTranscriptionResponse);
    //    }
    //}

    //[Test]
    //public void ProcessDataReceived_Should_Raise_MetaDataReceived_Event_When_Response_Contains_Type_LiveMetadataResponse()
    //{
    //    //Arrange
    //    var liveMetadataResponse = new AutoFaker<LiveMetadataResponse>().Generate();
    //    // ensure the right type is set for testing
    //    liveMetadataResponse.Type = Enums.LiveType.Metadata;
    //    var json = JsonSerializer.Serialize(liveMetadataResponse);
    //    var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json));

    //    MetadataReceivedEventArgs? eventArgs = null;
    //    _liveClient.MetadataReceived += (sender, args) => eventArgs = args;

    //    //Act
    //    _liveClient.ProcessDataReceived(_webSocketReceiveResult, memoryStream);

    //    //Assert
    //    using (new AssertionScope())
    //    {
    //        eventArgs.Should().NotBeNull();
    //        eventArgs!.MetaData.Should().NotBeNull();
    //        eventArgs.MetaData.Should().BeAssignableTo<LiveMetadataResponse>();
    //        eventArgs.MetaData.Should().BeEquivalentTo(liveMetadataResponse);
    //    }
    //}

    //[Test]
    //public void ProcessDataReceived_Should_Raise_LiveError_Event_When_Response_Contains_Unknown_Type()
    //{
    //    //Arrange
    //    var unknownDataResponse = new Dictionary<string, string>() { { "wiley", "coyote" } };
    //    var json = JsonSerializer.Serialize(unknownDataResponse);
    //    var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json));

    //    LiveErrorEventArgs? eventArgs = null;
    //    _liveClient.LiveError += (sender, args) => eventArgs = args;

    //    //Act
    //    _liveClient.ProcessDataReceived(_webSocketReceiveResult, memoryStream);

    //    //Assert
    //    using (new AssertionScope())
    //    {
    //        eventArgs.Should().NotBeNull();
    //        eventArgs!.Exception.Should().NotBeNull();
    //        eventArgs.Exception.Should().BeAssignableTo<Exception>();
    //    }
    //}


    #region Helpers
    [Test]
    public void GetBaseUrl_Should_Return_WSS_Protocol_When_DeepgramClientOptions_BaseAddress_Contains_No_Protocol()
    {
        //Arrange
        var expectedUrl = $"wss://{Defaults.DEFAULT_URI}";
        _options.BaseAddress = Defaults.DEFAULT_URI;
        //Act
        var result = LiveClient.GetBaseUrl(_options);

        //Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNullOrEmpty();
            result.Should().StartWith("wss://");
            result.Should().BeEquivalentTo(expectedUrl);
        }
    }

    [Test]
    public void GetBaseUrl_Should_Return_WSS_Protocol_When_BaseAddress_Contains_WS_Protocol()
    {
        //Arrange
        var expectedUrl = $"wss://{Defaults.DEFAULT_URI}";
        _options.BaseAddress = $"ws://{Defaults.DEFAULT_URI}";

        //Act
        var result = LiveClient.GetBaseUrl(_options);


        //Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNullOrEmpty();
            result.Should().StartWith("wss://");
            result.Should().BeEquivalentTo(expectedUrl);
        }
    }

    [Test]
    public void GetBaseUrl_Should_Return_WSS_Protocol_When_BaseAddress_Contains_WSS_Protocol()
    {
        //Arrange
        var expectedUrl = $"wss://{Defaults.DEFAULT_URI}";
        _options.BaseAddress = $"wss://{Defaults.DEFAULT_URI}";

        //Act
        var result = LiveClient.GetBaseUrl(_options);


        //Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNullOrEmpty();
            result.Should().StartWith("wss://");
            result.Should().BeEquivalentTo(expectedUrl);
        }
    }

    [Test]
    public void GetUri_Should_Return_Correctly_Formatted_Uri()
    {
        //Arrange
        var liveSchema = new LiveSchema()
        {
            Diarize = true,
        };
        _options.BaseAddress = Defaults.DEFAULT_URI;
        var expectedUriStart = $"wss://{Defaults.DEFAULT_URI}";
        var expectedQuery = $"{Defaults.API_VERSION}/{UriSegments.LISTEN}?diarize=true";
        var expectedCompleteUri = new Uri($"{expectedUriStart}/{expectedQuery}");
        //Act
        var result = LiveClient.GetUri(liveSchema, _options);

        //Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<Uri>();
            result.ToString().Should().StartWith(expectedUriStart);
            result.ToString().Should().Contain(expectedQuery);
            result.ToString().Should().BeEquivalentTo(expectedCompleteUri.ToString());
        }

    }

    #endregion
}