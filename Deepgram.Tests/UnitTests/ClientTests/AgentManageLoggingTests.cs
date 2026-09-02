// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;
using Deepgram.Logger;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.AgentManage.v1;
using Microsoft.Extensions.Logging;
using MelLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Deepgram.Tests.UnitTests.ClientTests;

/// <summary>
/// Regression tests for the Agent management logging contract: agent configurations (prompts,
/// metadata, function-endpoint headers) and variable values are customer data and must NEVER
/// appear in default SDK logs — not at Information and not at Debug. Only operation names and
/// resource IDs are logged. Verbose is the explicitly opt-in diagnostic level and is excluded
/// from the assertion.
/// </summary>
[NonParallelizable] // Log is a process-wide facade; recording must not interleave with other fixtures.
public class AgentManageLoggingTests
{
    // Sentinels planted in request payloads and in the stubbed API responses. If any of these
    // ever shows up at Information or Debug, payload logging has regressed.
    private const string PromptSentinel = "SENTINEL_PROMPT_do_not_log_7f3a";
    private const string HeaderSentinel = "SENTINEL_HEADER_VALUE_do_not_log_9b1c";
    private const string MetadataSentinel = "SENTINEL_METADATA_do_not_log_2e8d";
    private const string VariableValueSentinel = "SENTINEL_VARIABLE_VALUE_do_not_log_5c4f";

    private static readonly string[] Sentinels =
    {
        PromptSentinel, HeaderSentinel, MetadataSentinel, VariableValueSentinel,
    };

    private RecordingLoggerProvider _provider = null!;
    private DeepgramHttpClientOptions _options = null!;
    private string _apiKey = null!;
    private string _projectId = null!;

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

        _apiKey = new Faker().Random.Guid().ToString();
        _options = new DeepgramHttpClientOptions(_apiKey) { OnPrem = true };
        _projectId = new Faker().Random.Guid().ToString();
    }

    [TearDown]
    public void TearDown()
    {
        Log.Reset();
        _provider.Dispose();
    }

    private AgentManageClient NewClientReturning(string rawResponseBody)
    {
        var client = new AgentManageClient(_apiKey, _options);
        client._httpClient = MockHttpClient.CreateHttpClientWithRawResult(rawResponseBody, HttpStatusCode.OK);
        return client;
    }

    private void AssertNoSentinelAtInformationOrDebug()
    {
        var leaked = _provider.Entries
            .Where(e => e.Level == MelLogLevel.Information || e.Level == MelLogLevel.Debug)
            .Where(e => Sentinels.Any(s => e.Message.Contains(s)))
            .ToList();

        leaked.Should().BeEmpty(
            "agent configurations, prompts, endpoint headers, metadata, and variable values must never be logged at Information or Debug");
    }

    [Test]
    public async Task CreateAgent_Should_Not_Log_Configuration_Payload()
    {
        // The stubbed response also carries the sentinels, covering response-payload logging.
        var responseBody = """
        {
            "agent_uuid": "28f134a8-0967-45cb-a792-8cf8f729e586",
            "config": "{\"think\":{\"prompt\":\"__PROMPT__\"}}",
            "metadata": { "team": "__METADATA__" }
        }
        """
            .Replace("__PROMPT__", PromptSentinel)
            .Replace("__METADATA__", MetadataSentinel);
        var client = NewClientReturning(responseBody);

        var config = """
        {
            "think": {
                "provider": { "type": "open_ai", "model": "gpt-4o-mini" },
                "prompt": "__PROMPT__",
                "functions": [ { "endpoint": { "url": "https://example.com", "headers": { "authorization": "__HEADER__" } } } ]
            }
        }
        """
            .Replace("__PROMPT__", PromptSentinel)
            .Replace("__HEADER__", HeaderSentinel);

        await client.CreateAgent(_projectId, new AgentConfigurationSchema
        {
            Config = config,
            Metadata = new Dictionary<string, string> { ["team"] = MetadataSentinel },
        });

        AssertNoSentinelAtInformationOrDebug();
    }

    [Test]
    public async Task UpdateAgentMetadata_Should_Not_Log_Metadata_Payload()
    {
        var client = NewClientReturning("");

        await client.UpdateAgentMetadata(_projectId, "agent-1", new AgentMetadataSchema
        {
            Metadata = new Dictionary<string, string> { ["team"] = MetadataSentinel },
        });

        AssertNoSentinelAtInformationOrDebug();
    }

    [Test]
    public async Task CreateAgentVariable_Should_Not_Log_Variable_Value()
    {
        var responseBody = """
        {
            "agent_variable_uuid": "ec490d38-3cfc-4452-8a75-40dd14e69a7e",
            "key": "DG_GREETING",
            "value": "__VALUE__"
        }
        """.Replace("__VALUE__", VariableValueSentinel);
        var client = NewClientReturning(responseBody);

        await client.CreateAgentVariable(_projectId, new AgentVariableSchema
        {
            Key = "DG_GREETING",
            Value = VariableValueSentinel,
        });

        AssertNoSentinelAtInformationOrDebug();
    }

    [Test]
    public async Task UpdateAgentVariable_Should_Not_Log_Variable_Value()
    {
        var client = NewClientReturning("");

        await client.UpdateAgentVariable(_projectId, "variable-1", new UpdateAgentVariableSchema
        {
            Value = VariableValueSentinel,
        });

        AssertNoSentinelAtInformationOrDebug();
    }

    [Test]
    public async Task Get_Operations_Should_Not_Log_Response_Payloads()
    {
        var agentsBody = """
        [{
            "agent_uuid": "8f153566-fd4b-4ad4-bc13-09c66e0eed64",
            "config": "{\"think\":{\"prompt\":\"__PROMPT__\"}}",
            "metadata": { "team": "__METADATA__" }
        }]
        """
            .Replace("__PROMPT__", PromptSentinel)
            .Replace("__METADATA__", MetadataSentinel);
        var listClient = NewClientReturning(agentsBody);
        await listClient.GetAgents(_projectId);

        var variablesBody = """
        [{
            "agent_variable_uuid": "ec490d38-3cfc-4452-8a75-40dd14e69a7e",
            "key": "DG_GREETING",
            "value": "__VALUE__"
        }]
        """.Replace("__VALUE__", VariableValueSentinel);
        var variablesClient = NewClientReturning(variablesBody);
        await variablesClient.GetAgentVariables(_projectId);

        AssertNoSentinelAtInformationOrDebug();
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
