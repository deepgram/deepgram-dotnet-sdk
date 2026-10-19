// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;

using Deepgram.Logger;
using Deepgram.Models.Agent.v2.WebSocket;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Utilities;
using Microsoft.Extensions.Logging;
using MelLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Deepgram.Tests.UnitTests.ClientTests;

/// <summary>
/// Logging contract for the Voice Agent WebSocket connect path. Settings is the one connect
/// payload that carries secrets — provider credentials (AWS keys, STS session tokens) and
/// per-provider endpoint headers, which is where a third-party bearer token is supplied — and it
/// is logged on every Connect while Library.Initialize writes log.txt by default.
///
/// This mirrors the contract AgentManageLoggingTests already enforces on the Agent REST client:
/// credential values must never appear at ANY level, while non-secret fields must still be
/// logged so a silent no-op cannot pass.
/// </summary>
[NonParallelizable] // Log is a process-wide facade; recording must not interleave with other fixtures.
public class AgentCredentialLoggingTests
{
    private const string AccessKeySentinel = "SENTINEL_ACCESS_KEY_do_not_log_3a7f";
    private const string SecretKeySentinel = "SENTINEL_SECRET_KEY_do_not_log_8c2e";
    private const string SessionTokenSentinel = "SENTINEL_SESSION_TOKEN_do_not_log_5d9b";
    private const string EndpointHeaderSentinel = "SENTINEL_BEARER_do_not_log_1f4c";

    private static readonly string[] CredentialSentinels =
    {
        AccessKeySentinel, SecretKeySentinel, SessionTokenSentinel, EndpointHeaderSentinel,
    };

    private RecordingLoggerProvider _provider = null!;

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
    }

    [TearDown]
    public void TearDown()
    {
        Log.Reset();
        _provider.Dispose();
    }

    private static SettingsSchema SettingsWithSecrets()
    {
        var settings = new SettingsSchema();

        settings.Agent.Think = new Think
        {
            Provider = new AwsBedrockThinkProvider
            {
                Model = "anthropic/claude-3-5-haiku-20240307-v1:0",
                Credentials = new AwsCredentials
                {
                    Type = "sts",
                    Region = "us-east-1",
                    AccessKeyId = AccessKeySentinel,
                    SecretAccessKey = SecretKeySentinel,
                    SessionToken = SessionTokenSentinel,
                },
            },
            Endpoint = new Endpoint
            {
                URL = "https://api.openai.com/v1/chat/completions",
                Headers = new Dictionary<string, string> { { "authorization", EndpointHeaderSentinel } },
            },
        };

        settings.Agent.Speak = new Speak
        {
            Provider = new AwsPollySpeakProvider
            {
                Voice = "Joanna",
                Engine = "generative",
                Credentials = new AwsCredentials
                {
                    Type = "iam",
                    Region = "us-east-1",
                    AccessKeyId = AccessKeySentinel,
                    SecretAccessKey = SecretKeySentinel,
                },
            },
        };

        return settings;
    }

    [Test]
    public async Task Connect_Should_Not_Log_Provider_Credentials_At_Any_Level()
    {
        // Point at a closed port: Connect fails, but the options log line fires first, so the
        // real call site is exercised rather than the redaction helper in isolation.
        var options = new DeepgramWsClientOptions("a-key", "ws://127.0.0.1:1", keepAlive: false, onPrem: true);
        var client = new AgentWebSocketClient("a-key", options);

        try
        {
            await client.Connect(SettingsWithSecrets());
        }
        catch (Exception)
        {
            // Connection failure is expected and irrelevant; the log line is the subject.
        }

        var logged = string.Join("\n", _provider.Entries.Select(e => e.Message));

        using (new AssertionScope())
        {
            logged.Should().NotBeEmpty("the connect path must log something, or this test proves nothing");

            foreach (var sentinel in CredentialSentinels)
            {
                logged.Should().NotContain(sentinel, $"{sentinel} is a credential and must never be logged");
            }

            // Non-secret fields must survive, otherwise a blanket suppression would pass.
            logged.Should().Contain("anthropic/claude-3-5-haiku-20240307-v1:0");
            logged.Should().Contain("us-east-1");
            logged.Should().Contain(LogRedactionUtil.RedactedPlaceholder);
        }
    }

    [Test]
    public async Task Connect_Should_Still_Log_Settings_Without_Credentials()
    {
        // Back-compat: the overwhelmingly common case has no credentials, and its log output
        // must stay as useful as it was before redaction was added.
        var options = new DeepgramWsClientOptions("a-key", "ws://127.0.0.1:1", keepAlive: false, onPrem: true);
        var client = new AgentWebSocketClient("a-key", options);

        var settings = new SettingsSchema();
        settings.Agent.Listen = new Listen { Provider = new DeepgramListenProviderV2 { Model = "flux-general-en" } };

        try
        {
            await client.Connect(settings);
        }
        catch (Exception)
        {
        }

        var logged = string.Join("\n", _provider.Entries.Select(e => e.Message));

        using (new AssertionScope())
        {
            logged.Should().Contain("flux-general-en");
            logged.Should().NotContain(LogRedactionUtil.RedactedPlaceholder);
        }
    }

    // ---------- the redaction helper itself ----------

    [Test]
    public void SerializeRedacted_Should_Mask_Aws_Credential_Fields()
    {
        var json = LogRedactionUtil.SerializeRedacted(new AwsCredentials
        {
            Type = "sts",
            Region = "us-east-1",
            AccessKeyId = AccessKeySentinel,
            SecretAccessKey = SecretKeySentinel,
            SessionToken = SessionTokenSentinel,
        });

        using (new AssertionScope())
        {
            json.Should().NotContain(AccessKeySentinel);
            json.Should().NotContain(SecretKeySentinel);
            json.Should().NotContain(SessionTokenSentinel);
            // Type and region are diagnostic, not secret, and must survive.
            json.Should().Contain("sts").And.Contain("us-east-1");
        }
    }

    [Test]
    public void SerializeRedacted_Should_Mask_Endpoint_Header_Values_But_Keep_Names()
    {
        var json = LogRedactionUtil.SerializeRedacted(new Endpoint
        {
            URL = "https://api.openai.com/v1/chat/completions",
            Headers = new Dictionary<string, string>
            {
                { "authorization", EndpointHeaderSentinel },
                { "x-custom", "also-secret-by-convention" },
            },
        });

        using (new AssertionScope())
        {
            json.Should().NotContain(EndpointHeaderSentinel);
            json.Should().NotContain("also-secret-by-convention");
            // Header names and the URL stay, so a misconfigured endpoint is still debuggable.
            json.Should().Contain("authorization").And.Contain("x-custom");
            json.Should().Contain("api.openai.com");
        }
    }

    [Test]
    public void SerializeRedacted_Should_Reach_Credentials_Nested_In_Settings()
    {
        // The credential sits three levels down (agent > think > provider > credentials);
        // a shallow pass would miss it.
        var json = LogRedactionUtil.SerializeRedacted(SettingsWithSecrets());

        using (new AssertionScope())
        {
            foreach (var sentinel in CredentialSentinels)
            {
                json.Should().NotContain(sentinel);
            }

            json.Should().Contain("Joanna", "non-secret provider fields must survive");
        }
    }

    [Test]
    public void SerializeRedacted_Should_Handle_Null_And_Leave_Ordinary_Payloads_Intact()
    {
        using (new AssertionScope())
        {
            LogRedactionUtil.SerializeRedacted(null).Should().Be("null");

            var plain = LogRedactionUtil.SerializeRedacted(
                new DeepgramListenProviderV2 { Model = "flux-general-en", EotThreshold = 0.7 });
            plain.Should().Contain("flux-general-en");
            plain.Should().NotContain(LogRedactionUtil.RedactedPlaceholder);
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
