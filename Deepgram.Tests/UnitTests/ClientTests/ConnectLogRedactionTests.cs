// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;

using Deepgram.Logger;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Utilities;
using Microsoft.Extensions.Logging;
using MelLogLevel = Microsoft.Extensions.Logging.LogLevel;

using FluxSchema = Deepgram.Models.Flux.WebSocket.FluxSchema;
using FluxSpeakSchema = Deepgram.Models.Flux.Speak.WebSocket.SpeakSchema;
using ListenSchema = Deepgram.Models.Listen.v2.WebSocket.LiveSchema;
using LegacyListenSchema = Deepgram.Models.Listen.v1.WebSocket.LiveSchema;
using SpeakSchema = Deepgram.Models.Speak.v2.WebSocket.SpeakSchema;

namespace Deepgram.Tests.UnitTests.ClientTests;

/// <summary>
/// Every WebSocket client logs its connect payload at Information, and Library.Initialize writes
/// log.txt by default. The Agent client was the one that could demonstrably carry credentials and
/// is covered by AgentCredentialLoggingTests; these tests hold the same contract across the
/// remaining clients, so a future credential-bearing field cannot leak by being added to a schema
/// whose connect log was never hardened.
///
/// Only the Listen schemas expose a caller-controlled dictionary (Extra), so they are the only
/// ones where a sensitive key can actually be planted today. For the rest the assertion is that
/// the payload still logs usefully through the redacting path.
/// </summary>
[NonParallelizable] // Log is a process-wide facade; recording must not interleave with other fixtures.
public class ConnectLogRedactionTests
{
    private const string Sentinel = "SENTINEL_CONNECT_LOG_do_not_log_6b2d";
    private const string TagSecret = "TAGSECRET";
    private const string ExtraSecret = "EXTRASECRET";
    private const string CallbackPassword = "CBPASS";
    private const string CallbackToken = "CBTOKEN";

    private RecordingLoggerProvider _provider = null!;
    private DeepgramWsClientOptions _options = null!;

    [SetUp]
    public void SetUp()
    {
        Log.Reset();
        _provider = new RecordingLoggerProvider();
        Log.Configure(LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(MelLogLevel.Trace);
            builder.AddProvider(_provider);
        }));

        // A closed port: Connect fails, but the options log line fires first.
        _options = new DeepgramWsClientOptions("a-key", "ws://127.0.0.1:1", keepAlive: false, onPrem: true);
    }

    [TearDown]
    public void TearDown()
    {
        Log.Reset();
        _provider.Dispose();
    }

    private string Captured() => string.Join("\n", _provider.Entries.Select(e => e.Message));

    private static async Task TryConnect(Func<Task> connect)
    {
        try
        {
            await connect();
        }
        catch (Exception)
        {
            // Connection failure is expected; the log line is the subject.
        }
    }

    [Test]
    public async Task Listen_Connect_Should_Redact_A_Credential_Named_Extra_Key()
    {
        var client = new ListenWebSocketClient("a-key", _options);

        await TryConnect(() => client.Connect(new ListenSchema
        {
            Model = "nova-3",
            Extra = new Dictionary<string, string> { { "secret_access_key", Sentinel } },
        }));

        var logged = Captured();

        using (new AssertionScope())
        {
            logged.Should().NotBeEmpty();
            logged.Should().NotContain(Sentinel);
            logged.Should().Contain(LogRedactionUtil.RedactedPlaceholder);
            logged.Should().Contain("nova-3", "non-secret options must still be logged");
        }
    }

    [Test]
    public async Task Legacy_Listen_Connect_Should_Redact_A_Credential_Named_Extra_Key()
    {
        // The deprecated streaming client is still shipped and still used, so it gets the same
        // treatment rather than being left as the one leaking path.
#pragma warning disable CS0618 // deliberately exercising the deprecated client
        var client = new LiveClient("a-key", _options);
#pragma warning restore CS0618

        await TryConnect(() => client.Connect(new LegacyListenSchema
        {
            Model = "nova-3",
            Extra = new Dictionary<string, string> { { "session_token", Sentinel } },
        }));

        var logged = Captured();

        using (new AssertionScope())
        {
            logged.Should().NotContain(Sentinel);
            logged.Should().Contain(LogRedactionUtil.RedactedPlaceholder);
        }
    }

    [Test]
    public async Task Speak_Connect_Should_Still_Log_Options_Through_The_Redacting_Path()
    {
        var client = new SpeakWebSocketClient("a-key", _options);

        await TryConnect(() => client.Connect(new SpeakSchema { Model = "aura-2-thalia-en", SampleRate = 24000 }));

        using (new AssertionScope())
        {
            Captured().Should().Contain("aura-2-thalia-en");
            Captured().Should().NotContain(LogRedactionUtil.RedactedPlaceholder,
                "a payload with no credential-bearing field must log unchanged");
        }
    }

    [Test]
    public async Task Flux_Stt_Connect_Should_Still_Log_Options_Through_The_Redacting_Path()
    {
        var client = new FluxWebSocketClient("a-key", _options);

        await TryConnect(() => client.Connect(new FluxSchema { Model = "flux-general-en", EotThreshold = 0.7 }));

        using (new AssertionScope())
        {
            Captured().Should().Contain("flux-general-en");
            Captured().Should().NotContain(LogRedactionUtil.RedactedPlaceholder);
        }
    }

    [Test]
    public async Task Flux_Tts_Connect_Should_Still_Log_Options_Through_The_Redacting_Path()
    {
        var client = new FluxSpeakWebSocketClient("a-key", _options);

        await TryConnect(() => client.Connect(new FluxSpeakSchema { Model = "flux-kit-en", Speed = 1.05 }));

        using (new AssertionScope())
        {
            Captured().Should().Contain("flux-kit-en");
            Captured().Should().NotContain(LogRedactionUtil.RedactedPlaceholder);
        }
    }

    // ---------- the logged connect URL ----------

    [Test]
    public void RedactUri_Should_Mask_FreeForm_Parameters_But_Keep_Typed_Options()
    {
        // The connect URL carries every option as a query parameter and is logged at Debug.
        // extra and tag are free-form caller content, so a secret can sit in either and nothing
        // in the parameter name says so.
        var uri = new Uri($"wss://api.deepgram.com/v1/listen?model=nova-3&extra=secret_access_key%3a{Sentinel}&tag={Sentinel}&sample_rate=16000");

        var redacted = LogRedactionUtil.RedactUri(uri);

        using (new AssertionScope())
        {
            redacted.Should().NotContain(Sentinel);
            redacted.Should().Contain("model=nova-3", "typed options must stay readable");
            redacted.Should().Contain("sample_rate=16000");
            redacted.Should().Contain("api.deepgram.com/v1/listen");
        }
    }

    [Test]
    public void SerializeRedacted_Should_Mask_Sensitive_Container_Scalars_And_Arrays()
    {
        var payloads = new object[]
        {
            new Dictionary<string, object> { ["tag"] = new List<string> { TagSecret } },
            new Dictionary<string, object> { ["tag"] = TagSecret },
            new Dictionary<string, object> { ["extra"] = new List<string> { ExtraSecret } },
        };

        foreach (var payload in payloads)
        {
            var serialized = LogRedactionUtil.SerializeRedacted(payload);

            serialized.Should().NotContain(TagSecret).And.NotContain(ExtraSecret);
        }
    }

    [Test]
    public void Redaction_Should_Mask_Callback_Urls_In_Payloads_And_Connect_Urls()
    {
        var callback = $"https://user:{CallbackPassword}@example.com/hook?token={CallbackToken}";
        var payload = LogRedactionUtil.SerializeRedacted(new Dictionary<string, string> { ["callback"] = callback });
        var uri = LogRedactionUtil.RedactUri(new Uri($"wss://api.deepgram.com/v1/listen?callback={Uri.EscapeDataString(callback)}"));

        using (new AssertionScope())
        {
            payload.Should().NotContain(CallbackPassword).And.NotContain(CallbackToken);
            uri.Should().NotContain(CallbackPassword).And.NotContain(CallbackToken);
            uri.Should().Contain($"callback={LogRedactionUtil.RedactedPlaceholder}");
        }
    }

    private static readonly string[] CredentialNames =
    {
        "access_token", "api_key", "apikey", "token", "authorization", "password",
        "x-api-key", "client_secret", "secret",
    };

    [Test]
    public void RedactUri_Should_Mask_Credential_Named_Query_Parameters_In_Any_Case()
    {
        foreach (var name in CredentialNames)
        {
            foreach (var variant in new[] { name, name.ToUpperInvariant(), char.ToUpperInvariant(name[0]) + name.Substring(1) })
            {
                var uri = LogRedactionUtil.RedactUri(
                    new Uri($"wss://api.deepgram.com/v1/listen?model=nova-3&{variant}={Sentinel}&sample_rate=16000"));

                using (new AssertionScope())
                {
                    uri.Should().NotContain(Sentinel, $"{variant} is a credential name");
                    uri.Should().Contain($"{variant}={LogRedactionUtil.RedactedPlaceholder}");
                    uri.Should().Contain("model=nova-3").And.Contain("sample_rate=16000");
                }
            }
        }
    }

    [Test]
    public void RedactUri_Should_Keep_Lookalike_Query_Parameters_Readable()
    {
        var uri = LogRedactionUtil.RedactUri(
            new Uri("wss://api.deepgram.com/v1/listen?max_tokens=50&token_count=3&keywords=secretary&endpointing=300"));

        uri.Should().Contain("max_tokens=50").And.Contain("token_count=3")
            .And.Contain("keywords=secretary").And.Contain("endpointing=300");
    }

    [Test]
    public void SerializeRedacted_Should_Mask_Credential_Named_Keys_At_Top_Level_And_Nested()
    {
        foreach (var name in CredentialNames)
        {
            foreach (var variant in new[] { name, name.ToUpperInvariant() })
            {
                var payload = new Dictionary<string, object>
                {
                    [variant] = Sentinel,
                    ["model"] = "nova-3",
                    ["nested"] = new Dictionary<string, object> { [variant] = Sentinel, ["language"] = "en" },
                    ["list"] = new List<object> { new Dictionary<string, object> { [variant] = Sentinel } },
                };

                var serialized = LogRedactionUtil.SerializeRedacted(payload);

                using (new AssertionScope())
                {
                    serialized.Should().NotContain(Sentinel, $"{variant} is a credential name");
                    serialized.Should().Contain("nova-3").And.Contain("language");
                }
            }
        }
    }

    [Test]
    public void SerializeRedacted_Should_Mask_Object_And_Array_Values_Of_Credential_Names()
    {
        foreach (var name in CredentialNames)
        {
            var objectValue = LogRedactionUtil.SerializeRedacted(
                new Dictionary<string, object> { [name] = new Dictionary<string, object> { ["v"] = Sentinel } });
            var arrayValue = LogRedactionUtil.SerializeRedacted(
                new Dictionary<string, object> { [name] = new List<object> { Sentinel, new Dictionary<string, object> { ["v"] = Sentinel } } });

            using (new AssertionScope())
            {
                objectValue.Should().NotContain(Sentinel, $"{name} with an object value");
                arrayValue.Should().NotContain(Sentinel, $"{name} with an array value");
            }
        }
    }

    [Test]
    public void SerializeRedacted_Should_Keep_Lookalike_Keys_Readable()
    {
        var serialized = LogRedactionUtil.SerializeRedacted(new Dictionary<string, object>
        {
            ["max_tokens"] = 50,
            ["token_count"] = 3,
            ["secretary"] = "kept",
            ["model"] = "nova-3",
        });

        var parsed = System.Text.Json.Nodes.JsonNode.Parse(serialized)!;

        using (new AssertionScope())
        {
            ((int)parsed["max_tokens"]!).Should().Be(50);
            ((int)parsed["token_count"]!).Should().Be(3);
            ((string)parsed["secretary"]!).Should().Be("kept");
            ((string)parsed["model"]!).Should().Be("nova-3");
        }
    }

    [Test]
    public void SerializeRedacted_Should_Keep_Typed_Options_Readable()
    {
        var serialized = LogRedactionUtil.SerializeRedacted(new ListenSchema { Model = "flux-general-en", SampleRate = 16000 });

        serialized.Should().Contain("flux-general-en").And.Contain("16000");
    }

    [Test]
    public void RedactUri_Should_Leave_A_Query_Free_Url_Untouched()
    {
        var uri = new Uri("wss://agent.deepgram.com/v1/agent/converse");

        LogRedactionUtil.RedactUri(uri).Should().Be(uri.ToString());
    }

    [Test]
    public void RedactUri_Should_Mask_Rather_Than_Echo_An_Unparsable_Value()
    {
        using (new AssertionScope())
        {
            LogRedactionUtil.RedactUri($"not-a-url-{Sentinel}").Should().NotContain(Sentinel);
            LogRedactionUtil.RedactUri((string?)null).Should().Be("null");
            LogRedactionUtil.RedactUri((Uri?)null).Should().Be("null");
        }
    }

    private sealed class RecordingLoggerProvider : ILoggerProvider
    {
        public ConcurrentQueue<(MelLogLevel Level, string Category, string Message)> Entries { get; } = new();

        public ILogger CreateLogger(string categoryName) => new RecordingLogger(categoryName, this);

        public void Dispose() { }

        private sealed class RecordingLogger(string category, RecordingLoggerProvider provider) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(MelLogLevel logLevel) => logLevel != MelLogLevel.None;

            public void Log<TState>(MelLogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter)
                => provider.Entries.Enqueue((logLevel, category, formatter(state, exception)));
        }
    }
}
