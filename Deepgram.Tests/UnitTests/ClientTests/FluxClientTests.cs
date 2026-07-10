// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Globalization;
using System.Net.WebSockets;

using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Exceptions.v1;
using Deepgram.Models.Flux.v2.WebSocket;
using Deepgram.Clients.Flux.v2.WebSocket;
using Common = Deepgram.Models.Common.v2.WebSocket;
using FluxConstants = Deepgram.Clients.Flux.v2.WebSocket.Constants;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class FluxClientTests
{
    DeepgramWsClientOptions _options;
    string _apiKey;
    WebSocketReceiveResult _webSocketReceiveResult;

    [SetUp]
    public void Setup()
    {
        _apiKey = new Faker().Random.Guid().ToString();
        _options = new DeepgramWsClientOptions(_apiKey)
        {
            OnPrem = true,
        };
        _webSocketReceiveResult = new WebSocketReceiveResult(1, WebSocketMessageType.Text, true);
    }

    #region URI construction
    [Test]
    public void GetUri_With_Default_BaseAddress_Should_Route_To_V2_Listen()
    {
        var schema = new FluxSchema { Model = "flux-general-en" };

        var uri = Client.GetUri(_options, schema);

        uri.ToString().Should().StartWith("wss://api.deepgram.com/v2/listen?");
        uri.ToString().Should().Contain("model=flux-general-en");
        uri.ToString().Should().NotContain("/v1");
    }

    [Test]
    public void GetUri_With_Custom_BaseAddress_Should_Route_To_V2_Listen()
    {
        var options = new DeepgramWsClientOptions(_apiKey, "ws://localhost:8080") { OnPrem = true };
        var schema = new FluxSchema { Model = "flux-general-en" };

        var uri = Client.GetUri(options, schema);

        // DeepgramWsClientOptions appends the default /v1 API version to a custom base address;
        // the Flux client must strip it and route to v2/listen.
        uri.ToString().Should().StartWith("ws://localhost:8080/v2/listen?");
    }

    [Test]
    public void GetUri_With_Versioned_BaseAddress_Should_Replace_Version()
    {
        var options = new DeepgramWsClientOptions(_apiKey, "example.com/v3") { OnPrem = true };
        var schema = new FluxSchema { Model = "flux-general-en" };

        var uri = Client.GetUri(options, schema);

        uri.ToString().Should().StartWith("wss://example.com/v2/listen?");
        uri.ToString().Should().NotContain("/v3");
    }

    [Test]
    public void Connect_Without_Model_Should_Throw_DeepgramException()
    {
        var client = new FluxWebSocketClient(_apiKey, _options);

        Assert.ThrowsAsync<DeepgramException>(async () => await client.Connect(new FluxSchema()));
        Assert.ThrowsAsync<DeepgramException>(async () => await client.Connect(new FluxSchema { Model = "  " }));
    }
    #endregion

    #region Query string construction
    [Test]
    public void BuildQueryString_Should_Repeat_List_Parameters()
    {
        var schema = new FluxSchema
        {
            Model = "flux-general-en",
            Keyterm = new List<string> { "Deepgram", "flux general" },
        };

        var query = Client.BuildQueryString(schema);

        query.Should().Contain("keyterm=Deepgram");
        query.Should().Contain("keyterm=flux+general");
        query.Split('&').Count(p => p.StartsWith("keyterm=")).Should().Be(2);
    }

    [Test]
    public void BuildQueryString_Should_Lowercase_Booleans()
    {
        var schema = new FluxSchema
        {
            Model = "flux-general-en",
            MipOptOut = true,
            ProfanityFilter = false,
        };

        var query = Client.BuildQueryString(schema);

        query.Should().Contain("mip_opt_out=true");
        query.Should().Contain("profanity_filter=false");
    }

    [Test]
    public void BuildQueryString_Should_Format_Numbers_With_Invariant_Culture()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            // On a comma-decimal culture, a culture-sensitive ToString() would produce "0,7"
            // and the API would reject the parameter.
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            var schema = new FluxSchema
            {
                Model = "flux-general-en",
                EotThreshold = 0.7,
                EagerEotThreshold = 0.35,
                EotTimeoutMs = 5000,
                SampleRate = 16000,
            };

            var query = Client.BuildQueryString(schema);

            query.Should().Contain("eot_threshold=0.7");
            query.Should().Contain("eager_eot_threshold=0.35");
            query.Should().Contain("eot_timeout_ms=5000");
            query.Should().Contain("sample_rate=16000");
            query.Should().NotContain("0%2c7");
            query.Should().NotContain("0,7");
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Test]
    public void BuildQueryString_Should_Append_Addons()
    {
        var schema = new FluxSchema { Model = "flux-general-en" };
        var addons = new Dictionary<string, string> { { "future_param", "value" } };

        var query = Client.BuildQueryString(schema, addons);

        query.Should().Contain("future_param=value");
    }

    [Test]
    public void BuildQueryString_Should_Omit_Null_Parameters()
    {
        var schema = new FluxSchema { Model = "flux-general-en" };

        var query = Client.BuildQueryString(schema);

        query.Should().Be("model=flux-general-en");
    }
    #endregion

    #region Client -> server message serialization
    [Test]
    public void ControlMessage_CloseStream_Should_Serialize_To_Type_Only_Json()
    {
        var message = new ControlMessage(FluxConstants.CloseStream);

        using var doc = JsonDocument.Parse(message.ToString());

        doc.RootElement.GetProperty("type").GetString().Should().Be("CloseStream");
        doc.RootElement.EnumerateObject().Count().Should().Be(1);
    }

    [Test]
    public void ConfigureSchema_Should_Serialize_To_API_Shape()
    {
        var configure = new ConfigureSchema
        {
            Thresholds = new ConfigureThresholds
            {
                EotThreshold = 0.8,
                EagerEotThreshold = 0.5,
                EotTimeoutMs = 4000,
            },
            Keyterms = new List<string> { "Deepgram" },
            LanguageHints = new List<string> { "en", "es" },
        };

        using var doc = JsonDocument.Parse(configure.ToString());

        doc.RootElement.GetProperty("type").GetString().Should().Be("Configure");
        doc.RootElement.GetProperty("thresholds").GetProperty("eot_threshold").GetDouble().Should().Be(0.8);
        doc.RootElement.GetProperty("thresholds").GetProperty("eager_eot_threshold").GetDouble().Should().Be(0.5);
        doc.RootElement.GetProperty("thresholds").GetProperty("eot_timeout_ms").GetInt32().Should().Be(4000);
        doc.RootElement.GetProperty("keyterms")[0].GetString().Should().Be("Deepgram");
        doc.RootElement.GetProperty("language_hints")[1].GetString().Should().Be("es");
    }

    [Test]
    public void ConfigureSchema_Should_Omit_Unset_Parameters()
    {
        var configure = new ConfigureSchema
        {
            Thresholds = new ConfigureThresholds { EotThreshold = 0.6 },
        };

        using var doc = JsonDocument.Parse(configure.ToString());

        doc.RootElement.TryGetProperty("keyterms", out _).Should().BeFalse();
        doc.RootElement.TryGetProperty("language_hints", out _).Should().BeFalse();
        doc.RootElement.GetProperty("thresholds").TryGetProperty("eager_eot_threshold", out _).Should().BeFalse();
        doc.RootElement.GetProperty("thresholds").TryGetProperty("eot_timeout_ms", out _).Should().BeFalse();
    }

    [Test]
    public async Task SendConfigure_Should_Send_Message()
    {
        var fluxClient = Substitute.For<Client>(_apiKey, _options);
        fluxClient.When(x => x.SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<CancellationTokenSource>()))
                  .DoNotCallBase();

        await fluxClient.SendConfigure(new ConfigureSchema
        {
            Thresholds = new ConfigureThresholds { EotThreshold = 0.8 },
        });

        await fluxClient.Received(1).SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<CancellationTokenSource>());
    }

    [Test]
    public void SendConfigure_With_Null_Should_Throw_ArgumentNullException()
    {
        var client = new FluxWebSocketClient(_apiKey, _options);

        Assert.ThrowsAsync<ArgumentNullException>(async () => await client.SendConfigure(null!));
    }

    [Test]
    public async Task SendClose_When_Not_Connected_Should_Not_Throw()
    {
        var client = new FluxWebSocketClient(_apiKey, _options);

        await client.SendClose();
    }
    #endregion

    #region Server -> client message deserialization
    [Test]
    public void TurnInfoResponse_Should_Deserialize_All_Fields()
    {
        var json = """
        {
            "type": "TurnInfo",
            "request_id": "9d4b6c9e-0000-0000-0000-000000000000",
            "sequence_id": 7,
            "event": "EndOfTurn",
            "turn_index": 2,
            "audio_window_start": 1.5,
            "audio_window_end": 4.25,
            "transcript": "Hello world.",
            "words": [
                { "word": "Hello", "confidence": 0.97, "start": 1.5, "end": 2.0 },
                { "word": "world.", "confidence": 0.92, "start": 2.1, "end": 2.6 }
            ],
            "end_of_turn_confidence": 0.91,
            "languages": ["en"],
            "languages_hinted": ["en"]
        }
        """;

        var response = JsonSerializer.Deserialize<TurnInfoResponse>(json);

        using (new AssertionScope())
        {
            response!.Type.Should().Be(FluxType.TurnInfo);
            response.RequestId.Should().Be("9d4b6c9e-0000-0000-0000-000000000000");
            response.SequenceId.Should().Be(7);
            response.Event.Should().Be("EndOfTurn");
            response.EventType.Should().Be(TurnEvent.EndOfTurn);
            response.TurnIndex.Should().Be(2);
            response.AudioWindowStart.Should().Be(1.5);
            response.AudioWindowEnd.Should().Be(4.25);
            response.Transcript.Should().Be("Hello world.");
            response.Words.Should().HaveCount(2);
            response.Words![0].HeardWord.Should().Be("Hello");
            response.Words[0].Confidence.Should().Be(0.97);
            response.Words[0].Start.Should().Be(1.5);
            response.Words[0].End.Should().Be(2.0);
            response.EndOfTurnConfidence.Should().Be(0.91);
            response.Languages.Should().ContainSingle().Which.Should().Be("en");
            response.LanguagesHinted.Should().ContainSingle().Which.Should().Be("en");
        }
    }

    [Test]
    public void TurnInfoResponse_Should_Tolerate_Words_Without_Timestamps()
    {
        // Word-level start/end are optional on the wire; older responses omit them entirely.
        var json = """
        {
            "type": "TurnInfo",
            "request_id": "abc",
            "sequence_id": 1,
            "event": "Update",
            "turn_index": 0,
            "audio_window_start": 0.0,
            "audio_window_end": 1.0,
            "transcript": "hi",
            "words": [ { "word": "hi", "confidence": 1.0 } ],
            "end_of_turn_confidence": 0.1
        }
        """;

        var response = JsonSerializer.Deserialize<TurnInfoResponse>(json);

        using (new AssertionScope())
        {
            response!.Words.Should().ContainSingle();
            response.Words![0].HeardWord.Should().Be("hi");
            response.Words[0].Start.Should().BeNull();
            response.Words[0].End.Should().BeNull();
            response.Languages.Should().BeNull();
        }
    }

    [Test]
    public void TurnInfoResponse_Should_Tolerate_Unknown_Event_And_Extra_Fields()
    {
        // Forward compatibility: unknown turn events and additional properties must not break parsing.
        var json = """
        {
            "type": "TurnInfo",
            "request_id": "abc",
            "sequence_id": 1,
            "event": "SomeFutureEvent",
            "turn_index": 0,
            "audio_window_start": 0.0,
            "audio_window_end": 1.0,
            "transcript": "hi",
            "words": [],
            "end_of_turn_confidence": 0.1,
            "some_future_field": { "nested": true }
        }
        """;

        var response = JsonSerializer.Deserialize<TurnInfoResponse>(json);

        using (new AssertionScope())
        {
            response!.Event.Should().Be("SomeFutureEvent");
            response.EventType.Should().BeNull();
        }
    }

    [Test]
    public void ConnectedResponse_Should_Deserialize()
    {
        var json = """{ "type": "Connected", "request_id": "req-1", "sequence_id": 0 }""";

        var response = JsonSerializer.Deserialize<ConnectedResponse>(json);

        using (new AssertionScope())
        {
            response!.Type.Should().Be(FluxType.Connected);
            response.RequestId.Should().Be("req-1");
            response.SequenceId.Should().Be(0);
        }
    }

    [Test]
    public void ErrorResponse_Should_Deserialize_From_Wire_Type_Error()
    {
        // The fatal error message is named FatalError in the AsyncAPI schema but its wire
        // type is literally "Error" and it has no request_id.
        var json = """{ "type": "Error", "sequence_id": 12, "code": "INTERNAL_SERVER_ERROR", "description": "boom" }""";

        var response = JsonSerializer.Deserialize<ErrorResponse>(json);

        using (new AssertionScope())
        {
            response!.Type.Should().Be(FluxType.Error);
            response.SequenceId.Should().Be(12);
            response.Code.Should().Be("INTERNAL_SERVER_ERROR");
            response.Description.Should().Be("boom");
        }
    }

    [Test]
    public void ConfigureSuccessResponse_Should_Deserialize_With_Keyterms_Array()
    {
        var json = """
        {
            "type": "ConfigureSuccess",
            "request_id": "req-1",
            "sequence_id": 3,
            "thresholds": { "eager_eot_threshold": 0.5, "eot_threshold": 0.7, "eot_timeout_ms": 5000 },
            "keyterms": ["alpha", "beta"],
            "language_hints": ["en"]
        }
        """;

        var response = JsonSerializer.Deserialize<ConfigureSuccessResponse>(json);

        using (new AssertionScope())
        {
            response!.Type.Should().Be(FluxType.ConfigureSuccess);
            response.Thresholds!.EagerEotThreshold.Should().Be(0.5);
            response.Thresholds.EotThreshold.Should().Be(0.7);
            response.Thresholds.EotTimeoutMs.Should().Be(5000);
            response.Keyterms.Should().Equal("alpha", "beta");
            response.LanguageHints.Should().Equal("en");
        }
    }

    [Test]
    public void ConfigureSuccessResponse_Should_Deserialize_With_Keyterms_Single_String()
    {
        // The wire format allows keyterms to be a single string or an array of strings.
        var json = """
        {
            "type": "ConfigureSuccess",
            "request_id": "req-1",
            "sequence_id": 3,
            "thresholds": { "eot_threshold": 0.7 },
            "keyterms": "alpha"
        }
        """;

        var response = JsonSerializer.Deserialize<ConfigureSuccessResponse>(json);

        response!.Keyterms.Should().Equal("alpha");
    }

    [Test]
    public void ConfigureFailureResponse_Should_Deserialize_Both_Documented_Shapes()
    {
        // The AsyncAPI spec defines {type, request_id, sequence_id}; the docs show
        // {type, sequence_id, code, description}. Both must parse.
        var specShape = """{ "type": "ConfigureFailure", "request_id": "req-1", "sequence_id": 4 }""";
        var docsShape = """{ "type": "ConfigureFailure", "sequence_id": 4, "code": "BAD_CONFIG", "description": "nope" }""";

        var fromSpec = JsonSerializer.Deserialize<ConfigureFailureResponse>(specShape);
        var fromDocs = JsonSerializer.Deserialize<ConfigureFailureResponse>(docsShape);

        using (new AssertionScope())
        {
            fromSpec!.RequestId.Should().Be("req-1");
            fromSpec.SequenceId.Should().Be(4);
            fromSpec.Code.Should().BeNull();
            fromDocs!.RequestId.Should().BeNull();
            fromDocs.Code.Should().Be("BAD_CONFIG");
            fromDocs.Description.Should().Be("nope");
        }
    }
    #endregion

    #region Event dispatch (ProcessTextMessage)
    private static MemoryStream ToStream(string json) => new(Encoding.UTF8.GetBytes(json));

    [Test]
    public async Task ProcessTextMessage_Should_Raise_TurnInfo_Event()
    {
        var client = new FluxWebSocketClient(_apiKey, _options);
        TurnInfoResponse? received = null;
        await client.Subscribe(new EventHandler<TurnInfoResponse>((sender, e) => received = e));

        var json = """
        {
            "type": "TurnInfo", "request_id": "abc", "sequence_id": 5, "event": "StartOfTurn",
            "turn_index": 0, "audio_window_start": 0.0, "audio_window_end": 0.5,
            "transcript": "Hello", "words": [ { "word": "Hello", "confidence": 0.9 } ],
            "end_of_turn_confidence": 0.05
        }
        """;
        client.ProcessTextMessage(_webSocketReceiveResult, ToStream(json));

        using (new AssertionScope())
        {
            received.Should().NotBeNull();
            received!.EventType.Should().Be(TurnEvent.StartOfTurn);
            received.Transcript.Should().Be("Hello");
            received.SequenceId.Should().Be(5);
        }
    }

    [Test]
    public async Task ProcessTextMessage_Should_Raise_Connected_Event()
    {
        var client = new FluxWebSocketClient(_apiKey, _options);
        ConnectedResponse? received = null;
        await client.Subscribe(new EventHandler<ConnectedResponse>((sender, e) => received = e));

        client.ProcessTextMessage(_webSocketReceiveResult, ToStream(
            """{ "type": "Connected", "request_id": "req-1", "sequence_id": 0 }"""));

        received.Should().NotBeNull();
        received!.RequestId.Should().Be("req-1");
    }

    [Test]
    public async Task ProcessTextMessage_Should_Raise_ConfigureSuccess_Event()
    {
        var client = new FluxWebSocketClient(_apiKey, _options);
        ConfigureSuccessResponse? received = null;
        await client.Subscribe(new EventHandler<ConfigureSuccessResponse>((sender, e) => received = e));

        client.ProcessTextMessage(_webSocketReceiveResult, ToStream(
            """{ "type": "ConfigureSuccess", "request_id": "req-1", "sequence_id": 3, "thresholds": { "eot_threshold": 0.7 }, "keyterms": [] }"""));

        received.Should().NotBeNull();
        received!.Thresholds!.EotThreshold.Should().Be(0.7);
    }

    [Test]
    public async Task ProcessTextMessage_Should_Raise_ConfigureFailure_Event()
    {
        var client = new FluxWebSocketClient(_apiKey, _options);
        ConfigureFailureResponse? received = null;
        await client.Subscribe(new EventHandler<ConfigureFailureResponse>((sender, e) => received = e));

        client.ProcessTextMessage(_webSocketReceiveResult, ToStream(
            """{ "type": "ConfigureFailure", "request_id": "req-1", "sequence_id": 4 }"""));

        received.Should().NotBeNull();
        received!.SequenceId.Should().Be(4);
    }

    [Test]
    public async Task ProcessTextMessage_Should_Raise_Flux_Error_Event_For_Wire_Type_Error()
    {
        var client = new FluxWebSocketClient(_apiKey, _options);
        ErrorResponse? received = null;
        await client.Subscribe(new EventHandler<ErrorResponse>((sender, e) => received = e));

        client.ProcessTextMessage(_webSocketReceiveResult, ToStream(
            """{ "type": "Error", "sequence_id": 9, "code": "UNPARSABLE_CLIENT_MESSAGE", "description": "bad message" }"""));

        using (new AssertionScope())
        {
            received.Should().NotBeNull();
            received!.Code.Should().Be("UNPARSABLE_CLIENT_MESSAGE");
            received.SequenceId.Should().Be(9);
        }
    }

    [Test]
    public async Task ProcessTextMessage_Should_Raise_Unhandled_Event_For_Unknown_Type()
    {
        var client = new FluxWebSocketClient(_apiKey, _options);
        UnhandledResponse? received = null;
        await client.Subscribe(new EventHandler<UnhandledResponse>((sender, e) => received = e));

        var json = """{ "type": "SomeFutureMessage", "hello": "world" }""";
        client.ProcessTextMessage(_webSocketReceiveResult, ToStream(json));

        using (new AssertionScope())
        {
            received.Should().NotBeNull();
            received!.Type.Should().Be(Common.WebSocketType.Unhandled);
            received.Raw.Should().Contain("SomeFutureMessage");
        }
    }

    [Test]
    public async Task ProcessTextMessage_Should_Raise_Unhandled_Event_For_Missing_Type()
    {
        var client = new FluxWebSocketClient(_apiKey, _options);
        UnhandledResponse? received = null;
        await client.Subscribe(new EventHandler<UnhandledResponse>((sender, e) => received = e));

        client.ProcessTextMessage(_webSocketReceiveResult, ToStream("""{ "no_type": true }"""));

        received.Should().NotBeNull();
    }

    [Test]
    public async Task ProcessTextMessage_Should_Not_Throw_For_Invalid_Json()
    {
        var client = new FluxWebSocketClient(_apiKey, _options);
        await client.Subscribe(new EventHandler<TurnInfoResponse>((sender, e) => { }));

        client.ProcessTextMessage(_webSocketReceiveResult, ToStream("this is not json"));
    }

    [Test]
    public void ProcessBinaryMessage_Should_Not_Throw()
    {
        // The Flux API is not expected to send binary frames; the client must ignore them
        // rather than fault the receive loop.
        var client = new FluxWebSocketClient(_apiKey, _options);

        client.ProcessBinaryMessage(new WebSocketReceiveResult(1, WebSocketMessageType.Binary, true), new MemoryStream(new byte[] { 0x00 }));
    }
    #endregion

    #region Flux control-message guarantees
    [Test]
    public void FluxClient_Should_Not_Expose_KeepAlive_Or_Finalize()
    {
        // Flux rejects the v1-only KeepAlive/Finalize control messages with
        // UNPARSABLE_CLIENT_MESSAGE; the client and its interface must not offer them.
        using (new AssertionScope())
        {
            typeof(Client).GetMethod("SendKeepAlive").Should().BeNull();
            typeof(Client).GetMethod("SendFinalize").Should().BeNull();
            typeof(Deepgram.Clients.Interfaces.v2.IFluxWebSocketClient).GetMethod("SendKeepAlive").Should().BeNull();
            typeof(Deepgram.Clients.Interfaces.v2.IFluxWebSocketClient).GetMethod("SendFinalize").Should().BeNull();
        }
    }

    [Test]
    public void FluxClient_With_KeepAlive_Option_Should_Construct_And_Ignore_It()
    {
        // Reusing a DeepgramWsClientOptions with KeepAlive=true (a classic-listen setting)
        // must not enable any KeepAlive behavior on the Flux client.
        var options = new DeepgramWsClientOptions(_apiKey, null, keepAlive: true) { OnPrem = true };

        var client = new FluxWebSocketClient(_apiKey, options);

        client.Should().NotBeNull();
        // There is no KeepAlive thread to observe without a live socket; the guarantee that no
        // KeepAlive message can be produced is covered by FluxClient_Should_Not_Expose_KeepAlive_Or_Finalize
        // and by Connect() containing no KeepAlive/AutoFlush thread startup (see Client.Connect).
    }

    [Test]
    public void ClientFactory_Should_Create_FluxWebSocketClient()
    {
        var client = ClientFactory.CreateFluxWebSocketClient(_apiKey, _options);

        client.Should().NotBeNull();
        client.Should().BeAssignableTo<Deepgram.Clients.Interfaces.v2.IFluxWebSocketClient>();
        client.Should().BeOfType<FluxWebSocketClient>();
    }
    #endregion

    #region Connection lifecycle
    [Test]
    public async Task Connect_With_Cancelled_Token_Should_Return_False()
    {
        var client = new FluxWebSocketClient(_apiKey, _options);
        var cancelled = new CancellationTokenSource();
        cancelled.Cancel();

        var connected = await client.Connect(new FluxSchema { Model = "flux-general-en" }, cancelled);

        connected.Should().BeFalse();
    }

    [Test]
    public async Task Open_Wire_Message_Should_Be_Forwarded_To_Subscribers()
    {
        // NOTE: Close events are generated locally by Stop()/the receive loop, never sent by the
        // server as a text frame; a wire "Close" falls through to the base unhandled path exactly
        // like the classic listen client. The once-per-connection Close delivery guarantee is
        // asserted in FluxLiveIntegrationTests where a real connection teardown occurs.
        var client = new FluxWebSocketClient(_apiKey, _options);
        OpenResponse? open = null;
        var closeCount = 0;
        await client.Subscribe(new EventHandler<OpenResponse>((sender, e) => open = e));
        await client.Subscribe(new EventHandler<CloseResponse>((sender, e) => Interlocked.Increment(ref closeCount)));

        client.ProcessTextMessage(_webSocketReceiveResult, ToStream("""{ "type": "Open" }"""));
        client.ProcessTextMessage(_webSocketReceiveResult, ToStream("""{ "type": "Close" }"""));

        using (new AssertionScope())
        {
            open.Should().NotBeNull();
            closeCount.Should().Be(0, "Close events come from connection teardown, not wire messages");
        }
    }

    [Test]
    public void ProcessTextMessage_Without_Subscribers_Should_Not_Throw()
    {
        // Messages arriving with no listeners registered must be dropped quietly.
        var client = new FluxWebSocketClient(_apiKey, _options);

        var frames = new[]
        {
            """{ "type": "Connected", "request_id": "r", "sequence_id": 0 }""",
            """{ "type": "TurnInfo", "request_id": "r", "sequence_id": 1, "event": "Update", "turn_index": 0, "audio_window_start": 0, "audio_window_end": 1, "transcript": "", "words": [], "end_of_turn_confidence": 0 }""",
            """{ "type": "ConfigureSuccess", "request_id": "r", "sequence_id": 2, "thresholds": {}, "keyterms": [] }""",
            """{ "type": "ConfigureFailure", "request_id": "r", "sequence_id": 3 }""",
            """{ "type": "Error", "sequence_id": 4, "code": "X", "description": "y" }""",
            """{ "type": "SomethingElse" }""",
            """{ "no_type": true }""",
        };
        foreach (var frame in frames)
        {
            client.ProcessTextMessage(_webSocketReceiveResult, ToStream(frame));
        }
    }

    [Test]
    public async Task Stop_When_Never_Connected_Should_Return_True()
    {
        var client = new FluxWebSocketClient(_apiKey, _options);

        var result = await client.Stop();

        result.Should().BeTrue();
    }

    [Test]
    public void State_And_IsConnected_Without_Socket_Should_Report_Disconnected()
    {
        var client = new FluxWebSocketClient(_apiKey, _options);

        using (new AssertionScope())
        {
            client.State().Should().Be(WebSocketState.None);
            client.IsConnected().Should().BeFalse();
        }
    }
    #endregion

    #region Model serialization round-trips
    [Test]
    public void Response_Models_ToString_Should_RoundTrip_Through_Json()
    {
        var turnInfo = new TurnInfoResponse
        {
            RequestId = "req",
            SequenceId = 1,
            Event = "EndOfTurn",
            TurnIndex = 0,
            AudioWindowStart = 0.0,
            AudioWindowEnd = 2.0,
            Transcript = "hello",
            Words = new List<Word> { new() { HeardWord = "hello", Confidence = 0.9, Start = 0.5, End = 1.0 } },
            EndOfTurnConfidence = 0.8,
        };
        var connected = new ConnectedResponse { RequestId = "req", SequenceId = 0 };
        var error = new ErrorResponse { SequenceId = 2, Code = "X", Description = "y" };
        var failure = new ConfigureFailureResponse { SequenceId = 3, Code = "BAD", Description = "z" };
        var success = new ConfigureSuccessResponse
        {
            RequestId = "req",
            SequenceId = 4,
            Thresholds = new ConfigureThresholds { EagerEotThreshold = 0.4, EotThreshold = 0.7, EotTimeoutMs = 5000 },
            Keyterms = new List<string> { "alpha" },
        };
        var schema = new FluxSchema { Model = "flux-general-en", Keyterm = new List<string> { "a" } };

        using (new AssertionScope())
        {
            JsonSerializer.Deserialize<TurnInfoResponse>(turnInfo.ToString())!.Transcript.Should().Be("hello");
            JsonSerializer.Deserialize<ConnectedResponse>(connected.ToString())!.SequenceId.Should().Be(0);
            JsonSerializer.Deserialize<ErrorResponse>(error.ToString())!.Code.Should().Be("X");
            JsonSerializer.Deserialize<ConfigureFailureResponse>(failure.ToString())!.Code.Should().Be("BAD");
            var successRoundTrip = JsonSerializer.Deserialize<ConfigureSuccessResponse>(success.ToString())!;
            successRoundTrip.Keyterms.Should().Equal("alpha");
            successRoundTrip.Thresholds!.EotTimeoutMs.Should().Be(5000);
            JsonSerializer.Deserialize<FluxSchema>(schema.ToString())!.Model.Should().Be("flux-general-en");
            turnInfo.Words![0].ToString().Should().Contain("hello");
            success.Thresholds!.ToString().Should().Contain("eot_timeout_ms");
        }
    }

    [Test]
    public void StringOrStringListConverter_Should_Reject_Invalid_Json_Shapes()
    {
        using (new AssertionScope())
        {
            // keyterms as a number is not part of the wire contract
            var numberAct = () => JsonSerializer.Deserialize<ConfigureSuccessResponse>(
                """{ "type": "ConfigureSuccess", "keyterms": 42 }""");
            numberAct.Should().Throw<JsonException>();

            // an array with non-string elements is not part of the wire contract
            var mixedAct = () => JsonSerializer.Deserialize<ConfigureSuccessResponse>(
                """{ "type": "ConfigureSuccess", "keyterms": ["a", 1] }""");
            mixedAct.Should().Throw<JsonException>();

            // explicit null is tolerated
            var withNull = JsonSerializer.Deserialize<ConfigureSuccessResponse>(
                """{ "type": "ConfigureSuccess", "keyterms": null }""");
            withNull!.Keyterms.Should().BeNull();
        }
    }
    #endregion
}
