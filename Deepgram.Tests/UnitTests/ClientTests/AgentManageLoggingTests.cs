// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;
using Deepgram.Logger;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.AgentManage.v1;
using Deepgram.Models.Exceptions.v1;
using Microsoft.Extensions.Logging;
using MelLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Deepgram.Tests.UnitTests.ClientTests;

/// <summary>
/// Regression tests for the Agent management logging contract: agent configurations (prompts,
/// metadata, function-endpoint headers), variable values, caller-supplied header values
/// (Authorization included) and the client's own API key must NEVER appear in SDK logs at ANY
/// level — Information, Debug and Verbose/Trace alike. Only operation names, resource IDs and
/// header names are logged. Every one of the ten agent verbs is exercised with sentinels planted
/// in the request, the stubbed response and the request headers, and the failure path is covered
/// with sentinels planted in 4xx/5xx error bodies (which surface in exception messages).
///
/// A final test runs the same header/API-key assertion against a non-agent client to prove the
/// protection comes from the centralized REST header helper, not just the Agent override.
/// </summary>
[NonParallelizable] // Log is a process-wide facade; recording must not interleave with other fixtures.
public class AgentManageLoggingTests
{
    // Sentinels planted in request payloads, request headers and the stubbed API responses. If
    // any of these ever shows up at any level, payload or credential logging has regressed.
    private const string PromptSentinel = "SENTINEL_PROMPT_do_not_log_7f3a";
    private const string EndpointHeaderSentinel = "SENTINEL_ENDPOINT_HEADER_do_not_log_9b1c";
    private const string MetadataSentinel = "SENTINEL_METADATA_do_not_log_2e8d";
    private const string VariableValueSentinel = "SENTINEL_VARIABLE_VALUE_do_not_log_5c4f";
    private const string AuthorizationSentinel = "SENTINEL_AUTHORIZATION_do_not_log_4d2a";
    private const string CustomHeaderSentinel = "SENTINEL_CUSTOM_HEADER_VALUE_do_not_log_8e6b";
    private const string CustomHeaderName = "X-Sentinel-Header";
    private const string ErrorBodySentinel = "SENTINEL_ERROR_BODY_do_not_log_1c9e";

    private static readonly string[] PayloadSentinels =
    {
        PromptSentinel, EndpointHeaderSentinel, MetadataSentinel, VariableValueSentinel,
    };

    private static readonly string[] CredentialSentinels =
    {
        AuthorizationSentinel, CustomHeaderSentinel,
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

        _apiKey = "SENTINEL_API_KEY_do_not_log_" + new Faker().Random.Guid().ToString("N");
        _options = new DeepgramHttpClientOptions(_apiKey) { OnPrem = true };
        _projectId = new Faker().Random.Guid().ToString();
    }

    [TearDown]
    public void TearDown()
    {
        Log.Reset();
        _provider.Dispose();
    }

    private AgentManageClient NewAgentClientReturning(string rawResponseBody, HttpStatusCode status = HttpStatusCode.OK)
    {
        var client = new AgentManageClient(_apiKey, _options);
        client._httpClient = MockHttpClient.CreateHttpClientWithRawResult(rawResponseBody, status);
        return client;
    }

    /// <summary>
    /// Failure-path variant of <see cref="AssertNoSentinelAtAnyLevel"/>: the error body (and the
    /// exception message built from it) must not appear at any level, while the suppressed-path
    /// markers must, so a silent no-op cannot pass.
    /// </summary>
    private void AssertErrorBodySuppressedAtAnyLevel()
    {
        AssertNoCredentialAtAnyLevel();

        AllMessages().Where(m => m.Contains(ErrorBodySentinel)).Should().BeEmpty(
            "API error bodies can echo submitted agent data and must never be logged at any level for this client");
        _provider.Entries.Should().Contain(e => e.Level == MelLogLevel.Trace && e.Message.Contains("Deepgram Exception:") && e.Message.Contains("body logging disabled for this client"),
            "the error body must take the size-only log path");
        _provider.Entries.Should().Contain(e => e.Level == MelLogLevel.Error && e.Message.Contains("message suppressed (body logging disabled for this client)"),
            "the exception must be logged by type only, without its message");
    }

    /// <summary>
    /// Caller-supplied headers carrying credential-shaped values, passed via the public
    /// <c>headers:</c> parameter of every verb.
    /// </summary>
    private static Dictionary<string, string> SentinelHeaders() => new()
    {
        ["Authorization"] = $"Token {AuthorizationSentinel}",
        [CustomHeaderName] = CustomHeaderSentinel,
    };

    private IEnumerable<string> AllMessages() => _provider.Entries.Select(e => e.Message);

    private void AssertNoCredentialAtAnyLevel()
    {
        var leaked = AllMessages()
            .Where(m => CredentialSentinels.Any(m.Contains) || m.Contains(_apiKey))
            .ToList();

        leaked.Should().BeEmpty(
            "header values (Authorization included) and the API key must never be logged at any level");
    }

    private void AssertNoSentinelAtAnyLevel()
    {
        AssertNoCredentialAtAnyLevel();

        var leaked = AllMessages()
            .Where(m => PayloadSentinels.Any(m.Contains))
            .ToList();

        leaked.Should().BeEmpty(
            "agent configurations, prompts, endpoint headers, metadata and variable values must never be logged at any level, Trace included");

        // Positive checks: the shared helpers ran (header NAMES are logged at Debug, and the
        // response-body log took the suppressed path), so a silent no-op cannot pass this test.
        _provider.Entries.Should().Contain(e => e.Level == MelLogLevel.Debug && e.Message == $"Add Header {CustomHeaderName}",
            "header names are still logged at Debug");
        _provider.Entries.Should().Contain(e => e.Level == MelLogLevel.Trace && e.Message.Contains("body logging disabled for this client"),
            "the Agent client must take the size-only response log path");
    }

    private static string AgentBody(string uuid) => """
        {
            "agent_uuid": "__UUID__",
            "config": "{\"think\":{\"prompt\":\"__PROMPT__\",\"functions\":[{\"endpoint\":{\"headers\":{\"authorization\":\"__ENDPOINT_HEADER__\"}}}]}}",
            "metadata": { "team": "__METADATA__" }
        }
        """
        .Replace("__UUID__", uuid)
        .Replace("__PROMPT__", PromptSentinel)
        .Replace("__ENDPOINT_HEADER__", EndpointHeaderSentinel)
        .Replace("__METADATA__", MetadataSentinel);

    private static string VariableBody(string uuid) => """
        {
            "agent_variable_uuid": "__UUID__",
            "key": "DG_GREETING",
            "value": "__VALUE__"
        }
        """
        .Replace("__UUID__", uuid)
        .Replace("__VALUE__", VariableValueSentinel);

    private static string AgentConfig() => """
        {
            "think": {
                "provider": { "type": "open_ai", "model": "gpt-4o-mini" },
                "prompt": "__PROMPT__",
                "functions": [ { "endpoint": { "url": "https://example.com", "headers": { "authorization": "__ENDPOINT_HEADER__" } } } ]
            }
        }
        """
        .Replace("__PROMPT__", PromptSentinel)
        .Replace("__ENDPOINT_HEADER__", EndpointHeaderSentinel);

    // ---- Agent configurations: 5 verbs -------------------------------------------------------

    [Test]
    public async Task GetAgents_Should_Not_Log_Payload_Or_Credentials_At_Any_Level()
    {
        var client = NewAgentClientReturning($"[{AgentBody("8f153566-0000-0000-0000-000000000002")}]");

        await client.GetAgents(_projectId, headers: SentinelHeaders());

        AssertNoSentinelAtAnyLevel();
    }

    [Test]
    public async Task GetAgent_Should_Not_Log_Payload_Or_Credentials_At_Any_Level()
    {
        var client = NewAgentClientReturning(AgentBody("28f134a8-0000-0000-0000-000000000001"));

        await client.GetAgent(_projectId, "agent-1", headers: SentinelHeaders());

        AssertNoSentinelAtAnyLevel();
    }

    [Test]
    public async Task CreateAgent_Should_Not_Log_Payload_Or_Credentials_At_Any_Level()
    {
        var client = NewAgentClientReturning(AgentBody("28f134a8-0000-0000-0000-000000000001"));

        await client.CreateAgent(_projectId, new AgentConfigurationSchema
        {
            Config = AgentConfig(),
            Metadata = new Dictionary<string, string> { ["team"] = MetadataSentinel },
        }, headers: SentinelHeaders());

        AssertNoSentinelAtAnyLevel();
    }

    [Test]
    public async Task UpdateAgentMetadata_Should_Not_Log_Payload_Or_Credentials_At_Any_Level()
    {
        // The live API returns an empty body here; stub a populated one so the response path is
        // exercised with sentinels too.
        var client = NewAgentClientReturning(AgentBody("28f134a8-0000-0000-0000-000000000001"));

        await client.UpdateAgentMetadata(_projectId, "agent-1", new AgentMetadataSchema
        {
            Metadata = new Dictionary<string, string> { ["team"] = MetadataSentinel },
        }, headers: SentinelHeaders());

        AssertNoSentinelAtAnyLevel();
    }

    [Test]
    public async Task DeleteAgent_Should_Not_Log_Payload_Or_Credentials_At_Any_Level()
    {
        var client = NewAgentClientReturning($$"""{ "message": "{{MetadataSentinel}}" }""");

        await client.DeleteAgent(_projectId, "agent-1", headers: SentinelHeaders());

        AssertNoSentinelAtAnyLevel();
    }

    // ---- Agent variables: 5 verbs ------------------------------------------------------------

    [Test]
    public async Task GetAgentVariables_Should_Not_Log_Payload_Or_Credentials_At_Any_Level()
    {
        var client = NewAgentClientReturning($"[{VariableBody("ec490d38-0000-0000-0000-000000000003")}]");

        await client.GetAgentVariables(_projectId, headers: SentinelHeaders());

        AssertNoSentinelAtAnyLevel();
    }

    [Test]
    public async Task GetAgentVariable_Should_Not_Log_Payload_Or_Credentials_At_Any_Level()
    {
        var client = NewAgentClientReturning(VariableBody("ec490d38-0000-0000-0000-000000000003"));

        await client.GetAgentVariable(_projectId, "variable-1", headers: SentinelHeaders());

        AssertNoSentinelAtAnyLevel();
    }

    [Test]
    public async Task CreateAgentVariable_Should_Not_Log_Payload_Or_Credentials_At_Any_Level()
    {
        var client = NewAgentClientReturning(VariableBody("ec490d38-0000-0000-0000-000000000003"));

        await client.CreateAgentVariable(_projectId, new AgentVariableSchema
        {
            Key = "DG_GREETING",
            Value = VariableValueSentinel,
        }, headers: SentinelHeaders());

        AssertNoSentinelAtAnyLevel();
    }

    [Test]
    public async Task UpdateAgentVariable_Should_Not_Log_Payload_Or_Credentials_At_Any_Level()
    {
        var client = NewAgentClientReturning(VariableBody("ec490d38-0000-0000-0000-000000000003"));

        await client.UpdateAgentVariable(_projectId, "variable-1", new UpdateAgentVariableSchema
        {
            Value = VariableValueSentinel,
        }, headers: SentinelHeaders());

        AssertNoSentinelAtAnyLevel();
    }

    [Test]
    public async Task DeleteAgentVariable_Should_Not_Log_Payload_Or_Credentials_At_Any_Level()
    {
        var client = NewAgentClientReturning($$"""{ "message": "{{VariableValueSentinel}}" }""");

        await client.DeleteAgentVariable(_projectId, "variable-1", headers: SentinelHeaders());

        AssertNoSentinelAtAnyLevel();
    }

    // ---- Failure path: 4xx/5xx error bodies ----------------------------------------------------

    [Test]
    public async Task Failed_Agent_Call_Should_Not_Log_Structured_Error_Body_At_Any_Level()
    {
        // Management-style error body -> DeepgramRESTException whose Message is composed from it.
        var client = NewAgentClientReturning(
            $$"""{ "err_code": "BAD_REQUEST", "err_msg": "{{ErrorBodySentinel}}", "request_id": "req-1" }""",
            HttpStatusCode.BadRequest);

        await client.Invoking(c => c.CreateAgentVariable(_projectId, new AgentVariableSchema
        {
            Key = "DG_GREETING",
            Value = VariableValueSentinel,
        }, headers: SentinelHeaders())).Should().ThrowAsync<DeepgramException>();

        AssertErrorBodySuppressedAtAnyLevel();
        AllMessages().Where(m => m.Contains(VariableValueSentinel)).Should().BeEmpty();
    }

    [Test]
    public async Task Failed_Agent_Call_Should_Not_Log_Raw_Error_Body_At_Any_Level()
    {
        // Non-JSON error body -> generic DeepgramException whose Message IS the raw body.
        var client = NewAgentClientReturning(ErrorBodySentinel, HttpStatusCode.InternalServerError);

        await client.Invoking(c => c.GetAgents(_projectId, headers: SentinelHeaders()))
            .Should().ThrowAsync<DeepgramException>();

        AssertErrorBodySuppressedAtAnyLevel();
    }

    [Test]
    public async Task Failed_Agent_Delete_Should_Not_Log_Error_Body_At_Any_Level()
    {
        // Covers the empty-body-tolerant helper path (DeleteAllowingEmptyResponseAsync) on failure.
        var client = NewAgentClientReturning(
            $$"""{ "err_code": "NOT_FOUND", "err_msg": "{{ErrorBodySentinel}}" }""", HttpStatusCode.NotFound);

        await client.Invoking(c => c.DeleteAgentVariable(_projectId, "variable-1", headers: SentinelHeaders()))
            .Should().ThrowAsync<DeepgramException>();

        AssertErrorBodySuppressedAtAnyLevel();
    }

    // ---- Shared REST layer, non-agent client ---------------------------------------------------

    [Test]
    public async Task Shared_Rest_Client_Should_Log_Header_Names_But_Never_Values_Or_Api_Key()
    {
        // ManageClient does NOT suppress response bodies (that is the existing 7.0 diagnostic
        // contract), so only credentials are asserted here. This proves the centralized header
        // helper protects every REST client, not just the Agent override.
        var client = new ManageClient(_apiKey, _options);
        client._httpClient = MockHttpClient.CreateHttpClientWithRawResult(
            """{ "projects": [ { "project_id": "p1", "name": "demo" } ] }""", HttpStatusCode.OK);

        await client.GetProjects(headers: SentinelHeaders());

        AssertNoCredentialAtAnyLevel();
        _provider.Entries.Should().Contain(e => e.Level == MelLogLevel.Debug && e.Message == $"Add Header {CustomHeaderName}",
            "header names may be logged at Debug");
        _provider.Entries.Should().Contain(e => e.Level == MelLogLevel.Debug && e.Message == "Add Header Authorization",
            "the Authorization header NAME may be logged, its value never");
    }

    [Test]
    public async Task Shared_Rest_Client_Still_Logs_Error_Body_At_Verbose_But_Never_Credentials()
    {
        // Control: for a non-agent client the error body stays in the Verbose log (existing 7.0
        // diagnostic contract), proving the Agent behaviour is a per-client gate, not a blanket
        // removal. Credentials are still never logged.
        var client = new ManageClient(_apiKey, _options);
        client._httpClient = MockHttpClient.CreateHttpClientWithRawResult(
            $$"""{ "err_code": "BAD_REQUEST", "err_msg": "{{ErrorBodySentinel}}" }""", HttpStatusCode.BadRequest);

        await client.Invoking(c => c.GetProjects(headers: SentinelHeaders()))
            .Should().ThrowAsync<DeepgramException>();

        AssertNoCredentialAtAnyLevel();
        _provider.Entries.Should().Contain(e => e.Level == MelLogLevel.Trace && e.Message.Contains(ErrorBodySentinel),
            "non-agent clients keep the Verbose error-body log");
        _provider.Entries.Should().Contain(e => e.Level == MelLogLevel.Error && e.Message.Contains(ErrorBodySentinel),
            "non-agent clients keep the exception message at Error");
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
