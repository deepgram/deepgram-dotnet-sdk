using System.Net.WebSockets;
using Deepgram.DeepgramEventArgs;
using Deepgram.Models.Live.v1;
using Deepgram.Models.Authenticate.v1;

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

    [Test]
    public void ProcessDataReceived_Should_Raise_TranscriptReceived_Event_When_Response_Contains_Type_TranscriptionResponse()
    {
        //Arrange
        var liveTranscriptionResponse = new AutoFaker<TranscriptionResponse>().Generate();
        // ensure the right type is set for testing
        liveTranscriptionResponse.Type = Enums.LiveType.Results;
        var json = JsonSerializer.Serialize(liveTranscriptionResponse);
        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        ResponseReceivedEventArgs? eventArgs = null;

        _liveClient.EventResponseReceived += (sender, args) => eventArgs = args;


        //Act
        _liveClient.ProcessDataReceived(_webSocketReceiveResult, memoryStream);
        Task.Delay(5000);
        //Assert

        eventArgs.Should().NotBeNull();
        eventArgs!.Response.Transcription.Should().NotBeNull();
        eventArgs.Response.Transcription.Should().BeAssignableTo<TranscriptionResponse>();
    }

    [Test]
    public void ProcessDataReceived_Should_Raise_MetaDataReceived_Event_When_Response_Contains_Type_MetadataResponse()
    {
        //Arrange
        var liveMetadataResponse = new AutoFaker<MetadataResponse>().Generate();
        // ensure the right type is set for testing
        liveMetadataResponse.Type = Enums.LiveType.Metadata;
        var json = JsonSerializer.Serialize(liveMetadataResponse);
        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        ResponseReceivedEventArgs? eventArgs = null;

        _liveClient.EventResponseReceived += (sender, args) => eventArgs = args;

        //Act
        _liveClient.ProcessDataReceived(_webSocketReceiveResult, memoryStream);

        //Assert
        using (new AssertionScope())
        {
            eventArgs.Should().NotBeNull();
            eventArgs!.Response.MetaData.Should().NotBeNull();
            eventArgs.Response.MetaData.Should().BeAssignableTo<MetadataResponse>();
        }
    }

    [Test]
    public void ProcessDataReceived_Should_Raise_LiveError_Event_When_Response_Contains_Unknown_Type()
    {
        //Arrange
        var unknownDataResponse = new Dictionary<string, string>() { { "Wiley", "coyote" } };
        var json = JsonSerializer.Serialize(unknownDataResponse);
        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        ResponseReceivedEventArgs? eventArgs = null;

        _liveClient.EventResponseReceived += (sender, args) => eventArgs = args;

        //Act
        _liveClient.ProcessDataReceived(_webSocketReceiveResult, memoryStream);

        //Assert
        using (new AssertionScope())
        {
            eventArgs.Should().NotBeNull();
            eventArgs!.Response.Error.Should().NotBeNull();
            eventArgs.Response.Error.Should().BeAssignableTo<Exception>();
        }
    }


    #region Helpers
    [Test]
    public void GetBaseUrl_Should_Return_WSS_Protocol_When_DeepgramClientOptions_BaseAddress_Contains_No_Protocol()
    {
        //Arrange
        var expectedUrl = $"wss://{Defaults.DEFAULT_URI}";

        //Act
        var result = LiveClient.GetBaseUrl(_options);

        //Assert

        result.Should().NotBeNullOrEmpty();
        result.Should().StartWith("wss://");
        result.Should().BeEquivalentTo(expectedUrl);

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
        var expectedUriStart = $"wss://{Defaults.DEFAULT_URI}/v1";
        var expectedQuery = $"{UriSegments.LISTEN}?diarize=true";
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