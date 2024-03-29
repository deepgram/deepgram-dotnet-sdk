//// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
//// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
//// SPDX-License-Identifier: MIT

//using System.Net.WebSockets;

//using Deepgram.Models.Authenticate.v1;
//using Deepgram.Models.Live.v1;
//using Deepgram.Clients.Live.v1;

//namespace Deepgram.Tests.UnitTests.ClientTests;

//public class LiveClientTests
//{
//    DeepgramWsClientOptions _options;
//    WebSocketReceiveResult _webSocketReceiveResult;
//    LiveClient _liveClient;

//    [SetUp]
//    public void Setup()
//    {
//        var _apiKey = new Faker().Random.Guid().ToString();
//        _options = new DeepgramWsClientOptions(_apiKey, null, null, true);

//        _webSocketReceiveResult = new WebSocketReceiveResult(1, WebSocketMessageType.Text, true);
//        _liveClient = new LiveClient(_apiKey, _options);
//    }

//    [TearDown]
//    public void Teardown()
//    {
//        if (_liveClient != null)
//            _liveClient.Dispose();
//    }

//    //[Test]
//    //public void ProcessDataReceived_Should_Raise_TranscriptReceived_Event_When_Response_Contains_Type_TranscriptionResponse()
//    //{
//    //    // Input and Output
//    //    var liveTranscriptionResponse = new AutoFaker<TranscriptionResponse>().Generate();

//    //    // ensure the right type is set for testing
//    //    liveTranscriptionResponse.Type = LiveType.Results;
//    //    var json = JsonSerializer.Serialize(liveTranscriptionResponse);
//    //    var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json));

//    //    // Eventing
//    //    ResponseEventArgs? eventArgs = null;
//    //    _liveClient.EventResponseReceived += (sender, args) => eventArgs = args;

//    //    //Act
//    //    _liveClient.ProcessDataReceived(_webSocketReceiveResult, memoryStream);
//    //    Task.Delay(5000);

//    //    //Assert
//    //    eventArgs.Should().NotBeNull();
//    //    eventArgs!.Response.Transcription.Should().NotBeNull();
//    //    eventArgs.Response.Transcription.Should().BeAssignableTo<TranscriptionResponse>();
//    //}

//    //[Test]
//    //public void ProcessDataReceived_Should_Raise_MetaDataReceived_Event_When_Response_Contains_Type_MetadataResponse()
//    //{
//    //    // Input and Output
//    //    var liveMetadataResponse = new AutoFaker<MetadataResponse>().Generate();

//    //    // ensure the right type is set for testing
//    //    liveMetadataResponse.Type = LiveType.Metadata;
//    //    var json = JsonSerializer.Serialize(liveMetadataResponse);
//    //    var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json));

//    //    // Eventing
//    //    ResponseEventArgs? eventArgs = null;
//    //    _liveClient.EventResponseReceived += (sender, args) => eventArgs = args;

//    //    //Act
//    //    _liveClient.ProcessDataReceived(_webSocketReceiveResult, memoryStream);

//    //    //Assert
//    //    using (new AssertionScope())
//    //    {
//    //        eventArgs.Should().NotBeNull();
//    //        eventArgs!.Response.MetaData.Should().NotBeNull();
//    //        eventArgs.Response.MetaData.Should().BeAssignableTo<MetadataResponse>();
//    //    }
//    //}

//    //[Test]
//    //public void ProcessDataReceived_Should_Raise_LiveError_Event_When_Response_Contains_Unknown_Type()
//    //{
//    //    // Input and Output
//    //    var unknownDataResponse = new Dictionary<string, string>() { { "Wiley", "coyote" } };
//    //    var json = JsonSerializer.Serialize(unknownDataResponse);
//    //    var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json));

//    //    // Eventing
//    //    ResponseEventArgs? eventArgs = null;
//    //    _liveClient.EventResponseReceived += (sender, args) => eventArgs = args;

//    //    //Act
//    //    _liveClient.ProcessDataReceived(_webSocketReceiveResult, memoryStream);

//    //    //Assert
//    //    using (new AssertionScope())
//    //    {
//    //        eventArgs.Should().NotBeNull();
//    //        eventArgs!.Response.Error.Should().NotBeNull();
//    //        eventArgs.Response.Error.Should().BeAssignableTo<Exception>();
//    //    }
//    //}

//    //#region Helpers
//    //[Test]
//    //public void GetUri_Should_Return_Correctly_Formatted_Uri()
//    //{
//    //    // Input and Output
//    //    var liveSchema = new LiveSchema()
//    //    {
//    //        Diarize = true,
//    //    };
//    //    var expectedUriStart = $"wss://{Defaults.DEFAULT_URI}/v1";
//    //    var expectedQuery = $"{UriSegments.LISTEN}?diarize=true";
//    //    var expectedCompleteUri = new Uri($"{expectedUriStart}/{expectedQuery}");

//    //    //Act
//    //    var result = LiveClient.GetUri(_options, liveSchema);

//    //    //Assert
//    //    using (new AssertionScope())
//    //    {
//    //        result.Should().NotBeNull();
//    //        result.Should().BeAssignableTo<Uri>();
//    //        result.ToString().Should().StartWith(expectedUriStart);
//    //        result.ToString().Should().Contain(expectedQuery);
//    //        result.ToString().Should().BeEquivalentTo(expectedCompleteUri.ToString());
//    //    }
//    //}
//    //#endregion
//}