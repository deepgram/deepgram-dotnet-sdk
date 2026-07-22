// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Globalization;
using System.Net.WebSockets;

using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Exceptions.v1;
using Deepgram.Models.Flux.Speak.WebSocket;
using Deepgram.Clients.Flux.Speak.WebSocket;
using Common = Deepgram.Models.Common.v2.WebSocket;
using FluxSpeakConstants = Deepgram.Clients.Flux.Speak.WebSocket.Constants;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class FluxSpeakClientTests
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
    public void GetUri_With_Default_BaseAddress_Should_Route_To_V2_Speak()
    {
        var schema = new SpeakSchema { Model = "flux-alexis-en" };

        var uri = Client.GetUri(_options, schema);

        uri.ToString().Should().StartWith("wss://api.deepgram.com/v2/speak?");
        uri.ToString().Should().Contain("model=flux-alexis-en");
        uri.ToString().Should().NotContain("/v1");
    }

    [Test]
    public void GetUri_With_Custom_BaseAddress_Should_Route_To_V2_Speak()
    {
        var options = new DeepgramWsClientOptions(_apiKey, "ws://localhost:8080") { OnPrem = true };
        var schema = new SpeakSchema { Model = "flux-alexis-en" };

        var uri = Client.GetUri(options, schema);

        // DeepgramWsClientOptions appends the default /v1 API version to a custom base address;
        // the Flux TTS client must strip it and route to v2/speak.
        uri.ToString().Should().StartWith("ws://localhost:8080/v2/speak?");
    }

    [Test]
    public void GetUri_With_Versioned_BaseAddress_Should_Replace_Version()
    {
        var options = new DeepgramWsClientOptions(_apiKey, "example.com/v3") { OnPrem = true };
        var schema = new SpeakSchema { Model = "flux-alexis-en" };

        var uri = Client.GetUri(options, schema);

        uri.ToString().Should().StartWith("wss://example.com/v2/speak?");
        uri.ToString().Should().NotContain("/v3");
    }

    [Test]
    public void Connect_Without_Model_Should_Throw_DeepgramException()
    {
        var client = new FluxSpeakWebSocketClient(_apiKey, _options);

        Assert.ThrowsAsync<DeepgramException>(async () => await client.Connect(new SpeakSchema()));
        Assert.ThrowsAsync<DeepgramException>(async () => await client.Connect(new SpeakSchema { Model = "  " }));
    }
    #endregion

    #region Query string construction
    [Test]
    public void BuildQueryString_Should_Repeat_Tag_Parameters()
    {
        var schema = new SpeakSchema
        {
            Model = "flux-alexis-en",
            Tag = new List<string> { "production", "client xyz" },
        };

        var query = Client.BuildQueryString(schema);

        query.Should().Contain("tag=production");
        query.Should().Contain("tag=client+xyz");
        query.Split('&').Count(p => p.StartsWith("tag=")).Should().Be(2);
    }

    [Test]
    public void BuildQueryString_Should_Lowercase_Booleans()
    {
        var schema = new SpeakSchema { Model = "flux-alexis-en", MipOptOut = true };

        var query = Client.BuildQueryString(schema);

        query.Should().Contain("mip_opt_out=true");
    }

    [Test]
    public void BuildQueryString_Should_Format_SampleRate_As_Integer_Under_Comma_Decimal_Culture()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            // On a comma-decimal culture a mis-formatted number could carry a separator; an
            // integer sample rate must serialize as plain digits with no decimal point.
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            var schema = new SpeakSchema
            {
                Model = "flux-alexis-en",
                Encoding = "linear16",
                SampleRate = 44100,
            };

            var query = Client.BuildQueryString(schema);

            query.Should().Contain("sample_rate=44100");
            query.Should().Contain("encoding=linear16");
            query.Should().NotContain("44100.");
            query.Should().NotContain("44.100");
            query.Should().NotContain("44,100");
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Test]
    public void BuildQueryString_Should_Contain_Expected_Param_Names()
    {
        var schema = new SpeakSchema
        {
            Model = "flux-alexis-en",
            Encoding = "linear16",
            SampleRate = 24000,
            MipOptOut = false,
            Tag = new List<string> { "prod" },
        };

        var query = Client.BuildQueryString(schema);

        using (new AssertionScope())
        {
            query.Should().Contain("model=flux-alexis-en");
            query.Should().Contain("encoding=linear16");
            query.Should().Contain("sample_rate=24000");
            query.Should().Contain("mip_opt_out=false");
            query.Should().Contain("tag=prod");
        }
    }

    [Test]
    public void BuildQueryString_Should_Omit_Null_Parameters()
    {
        var schema = new SpeakSchema { Model = "flux-alexis-en" };

        var query = Client.BuildQueryString(schema);

        query.Should().Be("model=flux-alexis-en");
    }
    #endregion

    #region Client -> server message serialization
    [Test]
    public void SpeakMessage_Should_Serialize_To_Speak_Json()
    {
        var message = new SpeakMessage("Sure, I can help you cancel your subscription.");

        using var doc = JsonDocument.Parse(message.ToString());

        doc.RootElement.GetProperty("type").GetString().Should().Be("Speak");
        doc.RootElement.GetProperty("text").GetString().Should().Be("Sure, I can help you cancel your subscription.");
        doc.RootElement.EnumerateObject().Count().Should().Be(2);
    }

    [Test]
    public void ControlMessage_Flush_Should_Serialize_To_Type_Only_Json()
    {
        var message = new ControlMessage(FluxSpeakConstants.Flush);

        using var doc = JsonDocument.Parse(message.ToString());

        doc.RootElement.GetProperty("type").GetString().Should().Be("Flush");
        doc.RootElement.EnumerateObject().Count().Should().Be(1);
    }

    [Test]
    public void ControlMessage_Close_Should_Serialize_To_Close_Not_CloseStream()
    {
        var message = new ControlMessage(FluxSpeakConstants.Close);

        using var doc = JsonDocument.Parse(message.ToString());

        doc.RootElement.GetProperty("type").GetString().Should().Be("Close");
        // The STT (listen) endpoint uses "CloseStream"; TTS (speak) uses "Close". Guard against a
        // copy-paste regression.
        doc.RootElement.GetProperty("type").GetString().Should().NotBe("CloseStream");
    }

    [Test]
    public async Task SendText_Should_Send_Speak_Message()
    {
        var client = Substitute.For<Client>(_apiKey, _options);
        client.When(x => x.SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<CancellationTokenSource>()))
              .DoNotCallBase();

        await client.SendText("hello");

        await client.Received(1).SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<CancellationTokenSource>());
    }

    [Test]
    public async Task SendFlush_Should_Send_Flush_Message()
    {
        var client = Substitute.For<Client>(_apiKey, _options);
        client.When(x => x.SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<CancellationTokenSource>()))
              .DoNotCallBase();

        await client.SendFlush();

        await client.Received(1).SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<CancellationTokenSource>());
    }

    [Test]
    public void SendText_With_Null_Should_Throw_ArgumentNullException()
    {
        var client = new FluxSpeakWebSocketClient(_apiKey, _options);

        Assert.ThrowsAsync<ArgumentNullException>(async () => await client.SendText(null!));
    }

    [Test]
    public async Task SendClose_When_Not_Connected_Should_Not_Throw()
    {
        var client = new FluxSpeakWebSocketClient(_apiKey, _options);

        await client.SendClose();
    }
    #endregion

    #region Server -> client message deserialization
    [Test]
    public void ConnectedResponse_Should_Deserialize_With_Model_Uuids_Array()
    {
        var json = """
        {
            "type": "Connected",
            "request_id": "550e8400-e29b-41d4-a716-446655440000",
            "model_name": "flux-alexis-en",
            "model_version": "2026.06.01",
            "model_uuids": ["b3e47c20-9f81-4a2e-bd15-8d7c6e2a1f09"]
        }
        """;

        var response = JsonSerializer.Deserialize<ConnectedResponse>(json);

        using (new AssertionScope())
        {
            response!.Type.Should().Be(SpeakType.Connected);
            response.RequestId.Should().Be("550e8400-e29b-41d4-a716-446655440000");
            response.ModelName.Should().Be("flux-alexis-en");
            response.ModelVersion.Should().Be("2026.06.01");
            response.ModelUuids.Should().ContainSingle().Which.Should().Be("b3e47c20-9f81-4a2e-bd15-8d7c6e2a1f09");
        }
    }

    [Test]
    public void SpeechStartedResponse_Should_Deserialize()
    {
        var response = JsonSerializer.Deserialize<SpeechStartedResponse>(
            """{ "type": "SpeechStarted", "speech_id": "dg_sp_a1b2c3d4e5f6" }""");

        using (new AssertionScope())
        {
            response!.Type.Should().Be(SpeakType.SpeechStarted);
            response.SpeechId.Should().Be("dg_sp_a1b2c3d4e5f6");
        }
    }

    [Test]
    public void SpeechMetadataResponse_Should_Deserialize_With_Nested_ControlsApplied()
    {
        var json = """
        {
            "type": "SpeechMetadata",
            "speech_id": "dg_sp_a1b2c3d4e5f6",
            "audio_duration_ms": 3200,
            "input_character_count": 47,
            "billable_character_count": 45,
            "controls_applied": { "pronunciations_applied": 0, "pronunciation_warnings": 0 }
        }
        """;

        var response = JsonSerializer.Deserialize<SpeechMetadataResponse>(json);

        using (new AssertionScope())
        {
            response!.Type.Should().Be(SpeakType.SpeechMetadata);
            response.SpeechId.Should().Be("dg_sp_a1b2c3d4e5f6");
            response.AudioDurationMs.Should().Be(3200);
            response.InputCharacterCount.Should().Be(47);
            response.BillableCharacterCount.Should().Be(45);
            response.ControlsApplied.Should().NotBeNull();
            response.ControlsApplied!.PronunciationsApplied.Should().Be(0);
            response.ControlsApplied.PronunciationWarnings.Should().Be(0);
        }
    }

    [Test]
    public void FlushedResponse_Should_Deserialize()
    {
        var response = JsonSerializer.Deserialize<FlushedResponse>(
            """{ "type": "Flushed", "speech_id": "dg_sp_a1b2c3d4e5f6" }""");

        using (new AssertionScope())
        {
            response!.Type.Should().Be(SpeakType.Flushed);
            response.SpeechId.Should().Be("dg_sp_a1b2c3d4e5f6");
        }
    }

    [Test]
    public void SessionMetadataResponse_Should_Deserialize()
    {
        var json = """
        {
            "type": "SessionMetadata",
            "total_audio_duration_ms": 184500,
            "total_input_character_count": 4280,
            "total_billable_character_count": 4180
        }
        """;

        var response = JsonSerializer.Deserialize<SessionMetadataResponse>(json);

        using (new AssertionScope())
        {
            response!.Type.Should().Be(SpeakType.SessionMetadata);
            response.TotalAudioDurationMs.Should().Be(184500);
            response.TotalInputCharacterCount.Should().Be(4280);
            response.TotalBillableCharacterCount.Should().Be(4180);
        }
    }

    [Test]
    public void WarningResponse_Should_Deserialize()
    {
        var json = """{ "type": "Warning", "code": "NO_ACTIVE_SPEECH", "description": "There is no active turn. The request will be ignored." }""";

        var response = JsonSerializer.Deserialize<WarningResponse>(json);

        using (new AssertionScope())
        {
            response!.Type.Should().Be(SpeakType.Warning);
            response.Code.Should().Be("NO_ACTIVE_SPEECH");
            response.Description.Should().Be("There is no active turn. The request will be ignored.");
        }
    }

    [Test]
    public void ErrorResponse_Should_Deserialize_From_Wire_Type_Error()
    {
        var json = """{ "type": "Error", "code": "MESSAGE-0000", "description": "The message could not be parsed." }""";

        var response = JsonSerializer.Deserialize<ErrorResponse>(json);

        using (new AssertionScope())
        {
            response!.Type.Should().Be(SpeakType.Error);
            response.Code.Should().Be("MESSAGE-0000");
            response.Description.Should().Be("The message could not be parsed.");
        }
    }

    [Test]
    public void Responses_Should_Tolerate_Missing_And_Extra_Fields()
    {
        // Lenient parsing: unexpected/extra fields ignored, missing fields left null.
        using (new AssertionScope())
        {
            var connected = JsonSerializer.Deserialize<ConnectedResponse>(
                """{ "type": "Connected", "request_id": "r", "future_field": { "nested": true } }""");
            connected!.RequestId.Should().Be("r");
            connected.ModelUuids.Should().BeNull();

            var metadata = JsonSerializer.Deserialize<SpeechMetadataResponse>(
                """{ "type": "SpeechMetadata", "speech_id": "s" }""");
            metadata!.SpeechId.Should().Be("s");
            metadata.ControlsApplied.Should().BeNull();
            metadata.AudioDurationMs.Should().BeNull();
        }
    }
    #endregion

    #region Event dispatch (ProcessTextMessage / ProcessBinaryMessage)
    private static MemoryStream ToStream(string json) => new(Encoding.UTF8.GetBytes(json));

    [Test]
    public async Task ProcessTextMessage_Should_Raise_Connected_Event()
    {
        var client = new FluxSpeakWebSocketClient(_apiKey, _options);
        ConnectedResponse? received = null;
        await client.Subscribe(new EventHandler<ConnectedResponse>((sender, e) => received = e));

        client.ProcessTextMessage(_webSocketReceiveResult, ToStream(
            """{ "type": "Connected", "request_id": "req-1", "model_name": "flux-alexis-en", "model_version": "2026.06.01", "model_uuids": ["u1"] }"""));

        using (new AssertionScope())
        {
            received.Should().NotBeNull();
            received!.ModelName.Should().Be("flux-alexis-en");
            received.ModelUuids.Should().ContainSingle().Which.Should().Be("u1");
        }
    }

    [Test]
    public async Task ProcessTextMessage_Should_Raise_SpeechStarted_Event()
    {
        var client = new FluxSpeakWebSocketClient(_apiKey, _options);
        SpeechStartedResponse? received = null;
        await client.Subscribe(new EventHandler<SpeechStartedResponse>((sender, e) => received = e));

        client.ProcessTextMessage(_webSocketReceiveResult, ToStream(
            """{ "type": "SpeechStarted", "speech_id": "dg_sp_a1b2c3d4e5f6" }"""));

        received.Should().NotBeNull();
        received!.SpeechId.Should().Be("dg_sp_a1b2c3d4e5f6");
    }

    [Test]
    public async Task ProcessTextMessage_Should_Raise_SpeechMetadata_Event()
    {
        var client = new FluxSpeakWebSocketClient(_apiKey, _options);
        SpeechMetadataResponse? received = null;
        await client.Subscribe(new EventHandler<SpeechMetadataResponse>((sender, e) => received = e));

        client.ProcessTextMessage(_webSocketReceiveResult, ToStream(
            """{ "type": "SpeechMetadata", "speech_id": "s", "audio_duration_ms": 100, "input_character_count": 5, "billable_character_count": 5, "controls_applied": { "pronunciations_applied": 0, "pronunciation_warnings": 0 } }"""));

        using (new AssertionScope())
        {
            received.Should().NotBeNull();
            received!.AudioDurationMs.Should().Be(100);
            received.ControlsApplied!.PronunciationsApplied.Should().Be(0);
        }
    }

    [Test]
    public async Task ProcessTextMessage_Should_Raise_Flushed_Event()
    {
        var client = new FluxSpeakWebSocketClient(_apiKey, _options);
        FlushedResponse? received = null;
        await client.Subscribe(new EventHandler<FlushedResponse>((sender, e) => received = e));

        client.ProcessTextMessage(_webSocketReceiveResult, ToStream(
            """{ "type": "Flushed", "speech_id": "s" }"""));

        received.Should().NotBeNull();
        received!.SpeechId.Should().Be("s");
    }

    [Test]
    public async Task ProcessTextMessage_Should_Raise_SessionMetadata_Event()
    {
        var client = new FluxSpeakWebSocketClient(_apiKey, _options);
        SessionMetadataResponse? received = null;
        await client.Subscribe(new EventHandler<SessionMetadataResponse>((sender, e) => received = e));

        client.ProcessTextMessage(_webSocketReceiveResult, ToStream(
            """{ "type": "SessionMetadata", "total_audio_duration_ms": 1, "total_input_character_count": 2, "total_billable_character_count": 3 }"""));

        received.Should().NotBeNull();
        received!.TotalBillableCharacterCount.Should().Be(3);
    }

    [Test]
    public async Task ProcessTextMessage_Should_Raise_Warning_Event()
    {
        var client = new FluxSpeakWebSocketClient(_apiKey, _options);
        WarningResponse? received = null;
        await client.Subscribe(new EventHandler<WarningResponse>((sender, e) => received = e));

        client.ProcessTextMessage(_webSocketReceiveResult, ToStream(
            """{ "type": "Warning", "code": "NO_ACTIVE_SPEECH", "description": "no turn" }"""));

        received.Should().NotBeNull();
        received!.Code.Should().Be("NO_ACTIVE_SPEECH");
    }

    [Test]
    public async Task ProcessTextMessage_Should_Raise_Error_Event()
    {
        var client = new FluxSpeakWebSocketClient(_apiKey, _options);
        ErrorResponse? received = null;
        await client.Subscribe(new EventHandler<ErrorResponse>((sender, e) => received = e));

        client.ProcessTextMessage(_webSocketReceiveResult, ToStream(
            """{ "type": "Error", "code": "MESSAGE-0000", "description": "bad" }"""));

        received.Should().NotBeNull();
        received!.Code.Should().Be("MESSAGE-0000");
    }

    [Test]
    public async Task ProcessTextMessage_Should_Raise_Unhandled_For_Unknown_Type()
    {
        // Forward compatibility: a future message type must not throw, must not route to the error
        // event, and must be surfaced on the Unhandled event with its raw JSON.
        var client = new FluxSpeakWebSocketClient(_apiKey, _options);
        UnhandledResponse? unhandled = null;
        ErrorResponse? error = null;
        await client.Subscribe(new EventHandler<UnhandledResponse>((sender, e) => unhandled = e));
        await client.Subscribe(new EventHandler<ErrorResponse>((sender, e) => error = e));

        client.ProcessTextMessage(_webSocketReceiveResult, ToStream("""{ "type": "FutureMessage", "brand_new_field": 123 }"""));

        using (new AssertionScope())
        {
            unhandled.Should().NotBeNull();
            unhandled!.Type.Should().Be(Common.WebSocketType.Unhandled);
            unhandled.Raw.Should().Contain("FutureMessage");
            error.Should().BeNull("an unknown type must never route to the error event");
        }
    }

    [Test]
    public async Task ProcessTextMessage_Should_Raise_Unhandled_For_Missing_Type()
    {
        var client = new FluxSpeakWebSocketClient(_apiKey, _options);
        UnhandledResponse? received = null;
        await client.Subscribe(new EventHandler<UnhandledResponse>((sender, e) => received = e));

        client.ProcessTextMessage(_webSocketReceiveResult, ToStream("""{ "no_type": true }"""));

        received.Should().NotBeNull();
    }

    [Test]
    public async Task ProcessTextMessage_Should_Raise_Unhandled_For_NonString_Type()
    {
        // Forward compatibility: a future frame whose "type" is a number or object must not throw
        // (GetString() would otherwise raise InvalidOperationException and be swallowed, silently
        // dropping the frame). It must be surfaced on the Unhandled event, never the error event.
        var client = new FluxSpeakWebSocketClient(_apiKey, _options);
        UnhandledResponse? unhandled = null;
        ErrorResponse? error = null;
        await client.Subscribe(new EventHandler<UnhandledResponse>((sender, e) => unhandled = e));
        await client.Subscribe(new EventHandler<ErrorResponse>((sender, e) => error = e));

        client.ProcessTextMessage(_webSocketReceiveResult, ToStream("""{ "type": 5 }"""));
        client.ProcessTextMessage(_webSocketReceiveResult, ToStream("""{ "type": { "nested": true } }"""));

        using (new AssertionScope())
        {
            unhandled.Should().NotBeNull("a non-string type must be surfaced, not dropped");
            unhandled!.Type.Should().Be(Common.WebSocketType.Unhandled);
            error.Should().BeNull("a non-string type must never route to the error event");
        }
    }

    [Test]
    public async Task ProcessTextMessage_Should_Not_Throw_For_Invalid_Json()
    {
        var client = new FluxSpeakWebSocketClient(_apiKey, _options);
        await client.Subscribe(new EventHandler<ConnectedResponse>((sender, e) => { }));

        client.ProcessTextMessage(_webSocketReceiveResult, ToStream("this is not json"));
    }

    [Test]
    public void ProcessTextMessage_Without_Subscribers_Should_Not_Throw()
    {
        var client = new FluxSpeakWebSocketClient(_apiKey, _options);

        var frames = new[]
        {
            """{ "type": "Connected", "request_id": "r", "model_name": "m", "model_version": "v", "model_uuids": [] }""",
            """{ "type": "SpeechStarted", "speech_id": "s" }""",
            """{ "type": "SpeechMetadata", "speech_id": "s" }""",
            """{ "type": "Flushed", "speech_id": "s" }""",
            """{ "type": "SessionMetadata" }""",
            """{ "type": "Warning", "code": "X", "description": "y" }""",
            """{ "type": "Error", "code": "X", "description": "y" }""",
            """{ "type": "SomethingElse" }""",
            """{ "no_type": true }""",
        };
        foreach (var frame in frames)
        {
            client.ProcessTextMessage(_webSocketReceiveResult, ToStream(frame));
        }
    }

    [Test]
    public async Task ProcessBinaryMessage_Should_Raise_Audio_Event_With_Raw_Bytes()
    {
        // The critical binary/text split: audio bytes must be delivered as-is, never fed to the
        // JSON parser.
        var client = new FluxSpeakWebSocketClient(_apiKey, _options);
        AudioResponse? received = null;
        await client.Subscribe(new EventHandler<AudioResponse>((sender, e) => received = e));

        var audioBytes = new byte[] { 0x00, 0x01, 0x02, 0xFF, 0xFE, 0x7B }; // 0x7B is '{', would break a JSON parse
        client.ProcessBinaryMessage(new WebSocketReceiveResult(audioBytes.Length, WebSocketMessageType.Binary, true), new MemoryStream(audioBytes));

        using (new AssertionScope())
        {
            received.Should().NotBeNull();
            received!.Type.Should().Be(SpeakType.Audio);
            received.Stream.Should().NotBeNull();
            received.Stream!.ToArray().Should().Equal(audioBytes);
        }
    }

    [Test]
    public void ProcessBinaryMessage_Without_Subscriber_Should_Not_Throw()
    {
        var client = new FluxSpeakWebSocketClient(_apiKey, _options);

        client.ProcessBinaryMessage(new WebSocketReceiveResult(1, WebSocketMessageType.Binary, true), new MemoryStream(new byte[] { 0x00 }));
    }
    #endregion

    #region EA surface guarantees
    [Test]
    public void FluxSpeakClient_Should_Not_Expose_GA_Only_Or_V1_Members()
    {
        // KeepAlive/Finalize are v1-only; Interrupt/Configure/speed are GA-only. None are part of
        // the EA text-to-speech surface.
        using (new AssertionScope())
        {
            typeof(Client).GetMethod("SendKeepAlive").Should().BeNull();
            typeof(Client).GetMethod("SendFinalize").Should().BeNull();
            typeof(Client).GetMethod("SendInterrupt").Should().BeNull();
            typeof(Client).GetMethod("SendConfigure").Should().BeNull();

            var iface = typeof(Deepgram.Clients.Interfaces.v2.IFluxSpeakWebSocketClient);
            iface.GetMethod("SendKeepAlive").Should().BeNull();
            iface.GetMethod("SendFinalize").Should().BeNull();
            iface.GetMethod("SendInterrupt").Should().BeNull();
            iface.GetMethod("SendConfigure").Should().BeNull();

            // No speed parameter on the streaming schema.
            typeof(SpeakSchema).GetProperty("Speed").Should().BeNull();
        }
    }

    [Test]
    public void FluxSpeakClient_With_KeepAlive_Option_Should_Construct_And_Ignore_It()
    {
        var options = new DeepgramWsClientOptions(_apiKey, null, keepAlive: true) { OnPrem = true };

        var client = new FluxSpeakWebSocketClient(_apiKey, options);

        client.Should().NotBeNull();
    }

    [Test]
    public void ClientFactory_Should_Create_FluxSpeakWebSocketClient()
    {
        var client = ClientFactory.CreateFluxSpeakWebSocketClient(_apiKey, _options);

        using (new AssertionScope())
        {
            client.Should().NotBeNull();
            client.Should().BeAssignableTo<Deepgram.Clients.Interfaces.v2.IFluxSpeakWebSocketClient>();
            client.Should().BeOfType<FluxSpeakWebSocketClient>();
        }
    }
    #endregion

    #region Authentication
    [Test]
    public void Options_Should_Carry_ApiKey_And_AccessToken_For_WebSocket()
    {
        // Auth header emission is the shared AbstractWebSocketClient path (Authorization: token
        // <key> / Bearer <token>); here we assert both credential types are carried and that an
        // access token takes precedence, for the WebSocket transport.
        using (new AssertionScope())
        {
            var keyed = new DeepgramWsClientOptions("my-api-key") { OnPrem = true };
            keyed.ApiKey.Should().Be("my-api-key");

            var tokened = new DeepgramWsClientOptions(null, accessToken: "my-access-token") { OnPrem = true };
            tokened.AccessToken.Should().Be("my-access-token");

            // both usable to construct the client
            new FluxSpeakWebSocketClient("my-api-key", keyed).Should().NotBeNull();
            new FluxSpeakWebSocketClient("", tokened).Should().NotBeNull();
        }
    }
    #endregion

    #region Connection lifecycle
    [Test]
    public async Task Connect_With_Cancelled_Token_Should_Return_False()
    {
        var client = new FluxSpeakWebSocketClient(_apiKey, _options);
        var cancelled = new CancellationTokenSource();
        cancelled.Cancel();

        var connected = await client.Connect(new SpeakSchema { Model = "flux-alexis-en" }, cancelled);

        connected.Should().BeFalse();
    }

    [Test]
    public async Task Stop_When_Never_Connected_Should_Return_True()
    {
        var client = new FluxSpeakWebSocketClient(_apiKey, _options);

        var result = await client.Stop();

        result.Should().BeTrue();
    }

    [Test]
    public void State_And_IsConnected_Without_Socket_Should_Report_Disconnected()
    {
        var client = new FluxSpeakWebSocketClient(_apiKey, _options);

        using (new AssertionScope())
        {
            client.State().Should().Be(WebSocketState.None);
            client.IsConnected().Should().BeFalse();
        }
    }

    [Test]
    public async Task Open_Wire_Message_Should_Be_Forwarded_And_Wire_Close_Should_Not_Raise_Close()
    {
        // Open/Close text frames delegate to the base; a wire "Close" is not a real close event
        // (those come from connection teardown), so it must not reach Close subscribers.
        var client = new FluxSpeakWebSocketClient(_apiKey, _options);
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
    #endregion

    #region Model serialization round-trips
    [Test]
    public void Response_Models_ToString_Should_RoundTrip_Through_Json()
    {
        var connected = new ConnectedResponse { RequestId = "r", ModelName = "flux-alexis-en", ModelVersion = "v", ModelUuids = new List<string> { "u1" } };
        var speechStarted = new SpeechStartedResponse { SpeechId = "s" };
        var speechMetadata = new SpeechMetadataResponse
        {
            SpeechId = "s",
            AudioDurationMs = 100,
            InputCharacterCount = 5,
            BillableCharacterCount = 5,
            ControlsApplied = new ControlsApplied { PronunciationsApplied = 0, PronunciationWarnings = 0 },
        };
        var session = new SessionMetadataResponse { TotalAudioDurationMs = 1, TotalInputCharacterCount = 2, TotalBillableCharacterCount = 3 };
        var warning = new WarningResponse { Code = "X", Description = "y" };
        var error = new ErrorResponse { Code = "MESSAGE-0000", Description = "z" };
        var schema = new SpeakSchema { Model = "flux-alexis-en", Encoding = "linear16", SampleRate = 44100, Tag = new List<string> { "a" } };

        using (new AssertionScope())
        {
            JsonSerializer.Deserialize<ConnectedResponse>(connected.ToString())!.ModelUuids.Should().Equal("u1");
            JsonSerializer.Deserialize<SpeechStartedResponse>(speechStarted.ToString())!.SpeechId.Should().Be("s");
            JsonSerializer.Deserialize<SpeechMetadataResponse>(speechMetadata.ToString())!.ControlsApplied!.PronunciationsApplied.Should().Be(0);
            JsonSerializer.Deserialize<SessionMetadataResponse>(session.ToString())!.TotalBillableCharacterCount.Should().Be(3);
            JsonSerializer.Deserialize<WarningResponse>(warning.ToString())!.Code.Should().Be("X");
            JsonSerializer.Deserialize<ErrorResponse>(error.ToString())!.Code.Should().Be("MESSAGE-0000");
            JsonSerializer.Deserialize<SpeakSchema>(schema.ToString())!.SampleRate.Should().Be(44100);
        }
    }
    #endregion
}
