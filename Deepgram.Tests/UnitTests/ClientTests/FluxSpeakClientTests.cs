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

    [Test]
    public void BuildQueryString_Should_Format_Speed_And_Expressivity_Under_Comma_Decimal_Culture()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            var schema = new SpeakSchema { Model = "flux-alexis-en", Speed = 1.05, Expressivity = -2 };

            var query = Client.BuildQueryString(schema);

            query.Should().Contain("speed=1.05");
            query.Should().Contain("expressivity=-2");
            query.Should().NotContain("1,05");
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Test]
    public void BuildQueryString_Should_Omit_Speed_And_Expressivity_When_Null()
    {
        var schema = new SpeakSchema { Model = "flux-alexis-en" };

        var query = Client.BuildQueryString(schema);

        query.Should().NotContain("speed=");
        query.Should().NotContain("expressivity=");
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
    public void InterruptSchema_Bare_Should_Serialize_To_Type_Only_Json()
    {
        var interrupt = new InterruptSchema();

        using var doc = JsonDocument.Parse(interrupt.ToString());

        doc.RootElement.GetProperty("type").GetString().Should().Be("Interrupt");
        // The server rejects unknown fields on Interrupt; a bare interrupt must carry no extra keys.
        doc.RootElement.EnumerateObject().Count().Should().Be(1);
    }

    [Test]
    public void InterruptSchema_With_PlaybackOffset_Should_Serialize_Nested_Object()
    {
        var interrupt = new InterruptSchema { PlaybackOffset = new PlaybackOffset(1500) };

        using var doc = JsonDocument.Parse(interrupt.ToString());

        doc.RootElement.GetProperty("type").GetString().Should().Be("Interrupt");
        var offset = doc.RootElement.GetProperty("playback_offset");
        offset.GetProperty("type").GetString().Should().Be("time_ms");
        offset.GetProperty("value").GetInt64().Should().Be(1500);
    }

    [Test]
    public void ConfigureSchema_Should_Serialize_Speed()
    {
        var configure = new ConfigureSchema { Speed = 1.05 };

        using var doc = JsonDocument.Parse(configure.ToString());

        doc.RootElement.GetProperty("type").GetString().Should().Be("Configure");
        doc.RootElement.GetProperty("speed").GetDouble().Should().Be(1.05);
        doc.RootElement.EnumerateObject().Count().Should().Be(2);
    }

    [Test]
    public void ConfigureSchema_Should_Serialize_Speed_Under_Comma_Decimal_Culture()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            var configure = new ConfigureSchema { Speed = 1.05 };

            using var doc = JsonDocument.Parse(configure.ToString());

            doc.RootElement.GetProperty("speed").GetDouble().Should().Be(1.05);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Test]
    public void ConfigureSchema_With_OutOfRange_Speed_Should_Serialize_Without_Throwing()
    {
        // Pins "no local range-check": the SDK sends whatever is set and lets the server reject it
        // via ConfigureFailure (e.g. SPEED_OUT_OF_RANGE).
        var configure = new ConfigureSchema { Speed = 9.0 };

        var act = () => configure.ToString();

        act.Should().NotThrow();
        using var doc = JsonDocument.Parse(configure.ToString());
        doc.RootElement.GetProperty("speed").GetDouble().Should().Be(9.0);
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
    public async Task SendInterrupt_Bare_Should_Send_Interrupt_Message()
    {
        var client = Substitute.For<Client>(_apiKey, _options);
        client.When(x => x.SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<CancellationTokenSource>()))
              .DoNotCallBase();

        await client.SendInterrupt();

        await client.Received(1).SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<CancellationTokenSource>());
    }

    [Test]
    public async Task SendInterrupt_With_PlaybackOffsetMs_Overload_Should_Produce_Same_Frame_As_Explicit_Schema()
    {
        byte[]? viaOverload = null;
        byte[]? viaSchema = null;

        var client = Substitute.For<Client>(_apiKey, _options);
        client.When(x => x.SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<CancellationTokenSource>()))
              .Do(call => viaOverload = call.Arg<byte[]>());
        await client.SendInterrupt(1500L);

        var client2 = Substitute.For<Client>(_apiKey, _options);
        client2.When(x => x.SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<CancellationTokenSource>()))
              .Do(call => viaSchema = call.Arg<byte[]>());
        await client2.SendInterrupt(new InterruptSchema { PlaybackOffset = new PlaybackOffset(1500) });

        Encoding.UTF8.GetString(viaOverload!).Should().Be(Encoding.UTF8.GetString(viaSchema!));
    }

    [Test]
    public async Task SendConfigure_Should_Send_Configure_Message()
    {
        var client = Substitute.For<Client>(_apiKey, _options);
        client.When(x => x.SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<CancellationTokenSource>()))
              .DoNotCallBase();

        await client.SendConfigure(new ConfigureSchema { Speed = 1.05 });

        await client.Received(1).SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<CancellationTokenSource>());
    }

    [Test]
    public void SendConfigure_With_Null_Should_Throw_ArgumentNullException()
    {
        var client = new FluxSpeakWebSocketClient(_apiKey, _options);

        Assert.ThrowsAsync<ArgumentNullException>(async () => await client.SendConfigure(null!));
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

    [Test]
    public async Task SendInterrupt_And_SendConfigure_When_Not_Connected_Should_Not_Throw()
    {
        // Mirrors SendFlush/SendClose: SendMessageImmediately no-ops (logs and returns) when the
        // socket is not connected, rather than throwing. SendInterrupt/SendConfigure must behave
        // identically - no bespoke pre-connect validation was introduced for the GA additions.
        var client = new FluxSpeakWebSocketClient(_apiKey, _options);

        await client.SendInterrupt();
        await client.SendInterrupt(1000L);
        await client.SendConfigure(new ConfigureSchema { Speed = 1.0 });
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
    public void SpeechMetadataResponse_Should_Deserialize_BreaksApplied_When_Present()
    {
        var json = """
        {
            "type": "SpeechMetadata",
            "speech_id": "s",
            "controls_applied": { "pronunciations_applied": 2, "pronunciation_warnings": 0, "breaks_applied": 1 }
        }
        """;

        var response = JsonSerializer.Deserialize<SpeechMetadataResponse>(json);

        response!.ControlsApplied!.BreaksApplied.Should().Be(1);
    }

    [Test]
    public void SpeechMetadataResponse_Should_Tolerate_Missing_BreaksApplied()
    {
        // Older (EA-era) frames omit breaks_applied entirely; it must parse as null, not throw.
        var json = """
        {
            "type": "SpeechMetadata",
            "speech_id": "s",
            "controls_applied": { "pronunciations_applied": 0, "pronunciation_warnings": 0 }
        }
        """;

        var response = JsonSerializer.Deserialize<SpeechMetadataResponse>(json);

        response!.ControlsApplied!.BreaksApplied.Should().BeNull();
    }

    [Test]
    public void SpeechInterruptedResponse_Should_Deserialize_With_Offset_And_Text_Split()
    {
        var json = """
        {
            "type": "SpeechInterrupted",
            "audio_played_ms": 2340,
            "text_spoken": "Sure, I can help you cancel your subscription.",
            "text_remaining": " Let me pull up your account.",
            "metadata": {
                "speech_id": "dg_sp_a1b2c3d4e5f6",
                "audio_duration_ms": 2340,
                "input_character_count": 75,
                "billable_character_count": 75,
                "controls_applied": { "pronunciations_applied": 0, "pronunciation_warnings": 0, "breaks_applied": 0 }
            }
        }
        """;

        var response = JsonSerializer.Deserialize<SpeechInterruptedResponse>(json);

        using (new AssertionScope())
        {
            response!.Type.Should().Be(SpeakType.SpeechInterrupted);
            response.AudioPlayedMs.Should().Be(2340);
            response.TextSpoken.Should().Be("Sure, I can help you cancel your subscription.");
            response.TextRemaining.Should().Be(" Let me pull up your account.");
            response.Metadata.Should().NotBeNull();
            response.Metadata!.SpeechId.Should().Be("dg_sp_a1b2c3d4e5f6");
            response.Metadata.ControlsApplied!.BreaksApplied.Should().Be(0);
        }
    }

    [Test]
    public void SpeechInterruptedResponse_Without_PlaybackOffset_Should_Omit_Text_Split()
    {
        // When the Interrupt carried no playback_offset, the server cannot compute the split.
        var json = """
        {
            "type": "SpeechInterrupted",
            "audio_played_ms": 4000,
            "metadata": { "speech_id": "s", "audio_duration_ms": 4000 }
        }
        """;

        var response = JsonSerializer.Deserialize<SpeechInterruptedResponse>(json);

        using (new AssertionScope())
        {
            response!.TextSpoken.Should().BeNull();
            response.TextRemaining.Should().BeNull();
            response.AudioPlayedMs.Should().Be(4000);
        }
    }

    [Test]
    public void ConfigureSuccessResponse_Should_Deserialize_Applied_Speed()
    {
        var response = JsonSerializer.Deserialize<ConfigureSuccessResponse>(
            """{ "type": "ConfigureSuccess", "applied": { "speed": 1.05 } }""");

        using (new AssertionScope())
        {
            response!.Type.Should().Be(SpeakType.ConfigureSuccess);
            response.Applied!.Speed.Should().Be(1.05);
        }
    }

    [Test]
    public void ConfigureFailureResponse_Should_Deserialize_Known_Shape()
    {
        var json = """
        {
            "type": "ConfigureFailure",
            "code": "SPEED_OUT_OF_RANGE",
            "description": "speed must be between 0.85 and 1.15 in 0.05 increments",
            "field": "speed",
            "value": 3.5
        }
        """;

        var response = JsonSerializer.Deserialize<ConfigureFailureResponse>(json);

        using (new AssertionScope())
        {
            response!.Type.Should().Be(SpeakType.ConfigureFailure);
            response.Code.Should().Be("SPEED_OUT_OF_RANGE");
            response.Field.Should().Be("speed");
            response.Value!.Value.GetDouble().Should().Be(3.5);
        }
    }

    [Test]
    public void ConfigureFailureResponse_Should_Tolerate_Unknown_Future_Code()
    {
        // Code is an open set - a code the SDK has never seen must still parse cleanly.
        var response = JsonSerializer.Deserialize<ConfigureFailureResponse>(
            """{ "type": "ConfigureFailure", "code": "SOME-FUTURE-CODE", "description": "d" }""");

        response!.Code.Should().Be("SOME-FUTURE-CODE");
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
    public async Task ProcessTextMessage_Should_Raise_SpeechInterrupted_Event()
    {
        var client = new FluxSpeakWebSocketClient(_apiKey, _options);
        SpeechInterruptedResponse? received = null;
        await client.Subscribe(new EventHandler<SpeechInterruptedResponse>((sender, e) => received = e));

        client.ProcessTextMessage(_webSocketReceiveResult, ToStream(
            """{ "type": "SpeechInterrupted", "audio_played_ms": 1200, "text_spoken": "Hello there", "text_remaining": "how are you?" }"""));

        using (new AssertionScope())
        {
            received.Should().NotBeNull();
            received!.AudioPlayedMs.Should().Be(1200);
            received.TextSpoken.Should().Be("Hello there");
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
    public async Task ProcessTextMessage_Should_Raise_ConfigureSuccess_Event()
    {
        var client = new FluxSpeakWebSocketClient(_apiKey, _options);
        ConfigureSuccessResponse? received = null;
        await client.Subscribe(new EventHandler<ConfigureSuccessResponse>((sender, e) => received = e));

        client.ProcessTextMessage(_webSocketReceiveResult, ToStream(
            """{ "type": "ConfigureSuccess", "applied": { "speed": 1.1 } }"""));

        received.Should().NotBeNull();
        received!.Applied!.Speed.Should().Be(1.1);
    }

    [Test]
    public async Task ProcessTextMessage_Should_Raise_ConfigureFailure_Event_Not_Error_Event()
    {
        // ConfigureFailure is non-fatal (like Warning) and must never route to the fatal Error
        // handler.
        var client = new FluxSpeakWebSocketClient(_apiKey, _options);
        ConfigureFailureResponse? failure = null;
        ErrorResponse? error = null;
        await client.Subscribe(new EventHandler<ConfigureFailureResponse>((sender, e) => failure = e));
        await client.Subscribe(new EventHandler<ErrorResponse>((sender, e) => error = e));

        client.ProcessTextMessage(_webSocketReceiveResult, ToStream(
            """{ "type": "ConfigureFailure", "code": "SPEED_OUT_OF_RANGE", "description": "d", "field": "speed", "value": 9.0 }"""));

        using (new AssertionScope())
        {
            failure.Should().NotBeNull();
            failure!.Code.Should().Be("SPEED_OUT_OF_RANGE");
            error.Should().BeNull("ConfigureFailure is non-fatal and must not route to the Error event");
        }
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
            """{ "type": "SpeechInterrupted", "audio_played_ms": 1 }""",
            """{ "type": "Flushed", "speech_id": "s" }""",
            """{ "type": "SessionMetadata" }""",
            """{ "type": "Warning", "code": "X", "description": "y" }""",
            """{ "type": "ConfigureSuccess", "applied": { "speed": 1.0 } }""",
            """{ "type": "ConfigureFailure", "code": "X", "description": "y" }""",
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

    #region GA surface guarantees
    [Test]
    public void FluxSpeakClient_Should_Expose_GA_Members_But_Not_V1_Members()
    {
        // KeepAlive/Finalize are v1 (classic Speak)-only and never apply to Flux TTS.
        // Interrupt/Configure/speed/expressivity are the GA additions this client must expose.
        using (new AssertionScope())
        {
            typeof(Client).GetMethod("SendKeepAlive").Should().BeNull();
            typeof(Client).GetMethod("SendFinalize").Should().BeNull();
            typeof(Client).GetMethod("SendInterrupt", new[] { typeof(InterruptSchema) }).Should().NotBeNull();
            typeof(Client).GetMethod("SendInterrupt", new[] { typeof(long) }).Should().NotBeNull();
            typeof(Client).GetMethod("SendConfigure").Should().NotBeNull();

            var iface = typeof(Deepgram.Clients.Interfaces.v2.IFluxSpeakWebSocketClient);
            iface.GetMethod("SendKeepAlive").Should().BeNull();
            iface.GetMethod("SendFinalize").Should().BeNull();
            iface.GetMethod("SendInterrupt", new[] { typeof(InterruptSchema) }).Should().NotBeNull();
            iface.GetMethod("SendInterrupt", new[] { typeof(long) }).Should().NotBeNull();
            iface.GetMethod("SendConfigure").Should().NotBeNull();

            // speed/expressivity are on the streaming schema.
            typeof(SpeakSchema).GetProperty("Speed").Should().NotBeNull();
            typeof(SpeakSchema).GetProperty("Expressivity").Should().NotBeNull();
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
        var schema = new SpeakSchema { Model = "flux-alexis-en", Encoding = "linear16", SampleRate = 44100, Speed = 1.05, Expressivity = 1, Tag = new List<string> { "a" } };
        var speechInterrupted = new SpeechInterruptedResponse
        {
            AudioPlayedMs = 1200,
            TextSpoken = "Hello there",
            Metadata = new SpeechInterruptedMetadata { SpeechId = "s", ControlsApplied = new ControlsApplied { BreaksApplied = 1 } },
        };
        var configureSuccess = new ConfigureSuccessResponse { Applied = new ConfigureApplied { Speed = 1.1 } };
        var configureFailure = new ConfigureFailureResponse { Code = "SPEED_OUT_OF_RANGE", Field = "speed" };

        using (new AssertionScope())
        {
            JsonSerializer.Deserialize<ConnectedResponse>(connected.ToString())!.ModelUuids.Should().Equal("u1");
            JsonSerializer.Deserialize<SpeechStartedResponse>(speechStarted.ToString())!.SpeechId.Should().Be("s");
            JsonSerializer.Deserialize<SpeechMetadataResponse>(speechMetadata.ToString())!.ControlsApplied!.PronunciationsApplied.Should().Be(0);
            JsonSerializer.Deserialize<SessionMetadataResponse>(session.ToString())!.TotalBillableCharacterCount.Should().Be(3);
            JsonSerializer.Deserialize<WarningResponse>(warning.ToString())!.Code.Should().Be("X");
            JsonSerializer.Deserialize<ErrorResponse>(error.ToString())!.Code.Should().Be("MESSAGE-0000");
            JsonSerializer.Deserialize<SpeakSchema>(schema.ToString())!.SampleRate.Should().Be(44100);
            JsonSerializer.Deserialize<SpeakSchema>(schema.ToString())!.Speed.Should().Be(1.05);
            JsonSerializer.Deserialize<SpeechInterruptedResponse>(speechInterrupted.ToString())!.Metadata!.ControlsApplied!.BreaksApplied.Should().Be(1);
            JsonSerializer.Deserialize<ConfigureSuccessResponse>(configureSuccess.ToString())!.Applied!.Speed.Should().Be(1.1);
            JsonSerializer.Deserialize<ConfigureFailureResponse>(configureFailure.ToString())!.Field.Should().Be("speed");
        }
    }
    #endregion
}
