// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;
using Deepgram.Clients.Agent.v1.REST;
using Deepgram.Encapsulations;
using Deepgram.Logger;
using Deepgram.Models.Agent.v1.REST;
using Deepgram.Models.Authenticate.v1;
using Microsoft.Extensions.Logging;
using AgentCatalogRestClient = Deepgram.Clients.Agent.v1.REST.Client;
using MelLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class AgentCatalogClientTests
{
    private const string ApiKey = "test-api-key";

    [Test]
    public void GetUri_Should_Target_Agent_Host_For_Default_Options()
    {
        var options = new DeepgramHttpClientOptions(ApiKey);

        AgentCatalogRestClient.GetUri(options).Should().Be("https://agent.deepgram.com/v1/agent/settings/think/models");
    }

    [Test]
    public void GetUri_Should_Preserve_Custom_Host_And_Use_Agent_Version_Path()
    {
        var options = new DeepgramHttpClientOptions(ApiKey, "https://catalog.example.test/v3");

        AgentCatalogRestClient.GetUri(options).Should().Be("https://catalog.example.test/v1/agent/settings/think/models");
    }

    [Test]
    [NonParallelizable]
    public void CreateAgentCatalogClient_Should_Allow_A_Public_Catalog_Request_Without_Credentials()
    {
        var previousApiKey = Environment.GetEnvironmentVariable(Deepgram.Constants.Defaults.DEEPGRAM_API_KEY);
        var previousAccessToken = Environment.GetEnvironmentVariable(Deepgram.Constants.Defaults.DEEPGRAM_ACCESS_TOKEN);
        Environment.SetEnvironmentVariable(Deepgram.Constants.Defaults.DEEPGRAM_API_KEY, null);
        Environment.SetEnvironmentVariable(Deepgram.Constants.Defaults.DEEPGRAM_ACCESS_TOKEN, null);

        try
        {
            Action create = () => ClientFactory.CreateAgentCatalogClient();

            create.Should().NotThrow();
        }
        finally
        {
            Environment.SetEnvironmentVariable(Deepgram.Constants.Defaults.DEEPGRAM_API_KEY, previousApiKey);
            Environment.SetEnvironmentVariable(Deepgram.Constants.Defaults.DEEPGRAM_ACCESS_TOKEN, previousAccessToken);
        }
    }

    [Test]
    public async Task GetThinkModels_Should_Call_GetAsync_Returning_Catalog_Response()
    {
        var options = new DeepgramHttpClientOptions(ApiKey) { OnPrem = true };
        var expected = new AgentThinkModelsResponse
        {
            Models = new List<AgentThinkModel>
            {
                new() { Id = "gpt-4o-mini", Name = "GPT-4o mini", Provider = "open_ai" },
            },
        };
        var client = Substitute.For<AgentCatalogRestClient>(ApiKey, options, null);
        var uri = AgentCatalogRestClient.GetUri(options);

        client.When(x => x.GetAsync<AgentThinkModelsResponse>(Arg.Any<string>())).DoNotCallBase();
        client.GetAsync<AgentThinkModelsResponse>(uri).Returns(expected);

        var result = await client.GetThinkModels();

        await client.Received(1).GetAsync<AgentThinkModelsResponse>(uri);
        result.Should().BeSameAs(expected);
    }

    [Test]
    public void AgentThinkModelsResponse_Should_Deserialize_All_Catalog_Fields()
    {
        var response = JsonSerializer.Deserialize<AgentThinkModelsResponse>(
            """{"models":[{"id":"gpt-4o-mini","name":"GPT-4o mini","provider":"open_ai"},{"id":"claude-sonnet-4-20250514","name":"Claude Sonnet 4","provider":"anthropic"}]}""");

        response!.Models.Should().BeEquivalentTo(
            new[]
            {
                new AgentThinkModel { Id = "gpt-4o-mini", Name = "GPT-4o mini", Provider = "open_ai" },
                new AgentThinkModel { Id = "claude-sonnet-4-20250514", Name = "Claude Sonnet 4", Provider = "anthropic" },
            });
    }

    [Test]
    public void AgentThinkModelsResponse_Should_Deserialize_Recorded_Catalog_Fixture()
    {
        var response = JsonSerializer.Deserialize<AgentThinkModelsResponse>(ReadThinkModelsFixture());

        response!.Models.Should().BeEquivalentTo(ExpectedFixtureModels, options => options.WithStrictOrdering());
    }

    [Test]
    [NonParallelizable]
    public async Task GetThinkModels_Without_Credentials_Should_Send_No_Authorization_Header_And_Log_No_Warning()
    {
        using var credentials = new ClearedCredentials();
        var provider = new RecordingLoggerProvider();
        Log.Reset();
        Log.Configure(LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(MelLogLevel.Trace);
            builder.AddProvider(provider);
        }));

        try
        {
            var client = (AgentCatalogClient)ClientFactory.CreateAgentCatalogClient();
            var handler = new CapturingHttpMessageHandler(ReadThinkModelsFixture());
            client._httpClient = HttpClientFactory.ConfigureDeepgram(new HttpClient(handler), client._options);

            var result = await client.GetThinkModels();

            handler.Request.Should().NotBeNull();
            handler.Request!.Headers.Authorization.Should().BeNull();
            handler.Request.RequestUri!.AbsoluteUri.Should().Be("https://agent.deepgram.com/v1/agent/settings/think/models");
            result.Models.Should().BeEquivalentTo(ExpectedFixtureModels, options => options.WithStrictOrdering());
            provider.Entries.Should().NotContain(entry => entry.Level >= MelLogLevel.Warning);
        }
        finally
        {
            Log.Reset();
        }
    }

    [Test]
    [NonParallelizable]
    public void Other_Clients_Without_Credentials_Should_Still_Throw_Or_Warn()
    {
        using var credentials = new ClearedCredentials();
        var provider = new RecordingLoggerProvider();
        Log.Reset();
        Log.Configure(LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(MelLogLevel.Trace);
            builder.AddProvider(provider);
        }));

        try
        {
            Action cloud = () => ClientFactory.CreateListenRESTClient();
            cloud.Should().Throw<ArgumentException>();

            ClientFactory.CreateListenRESTClient(options: new DeepgramHttpClientOptions(onPrem: true));
            provider.Entries.Should().Contain(entry =>
                entry.Level == MelLogLevel.Warning &&
                entry.Message.Contains("No authentication credentials provided"));
        }
        finally
        {
            Log.Reset();
        }
    }

    [Test]
    public async Task Live_GetThinkModels_Should_Return_The_Current_Catalog()
    {
        var apiKey = GlobalTestEnvironment.DeepgramApiKeyAtStartup
            ?? Environment.GetEnvironmentVariable("DEEPGRAM_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Assert.Ignore("DEEPGRAM_API_KEY is not set. Skipping live Agent catalog test.");
        }

        var client = ClientFactory.CreateAgentCatalogClient(apiKey!);
        var result = await client.GetThinkModels();

        result.Models.Should().NotBeNullOrEmpty();
        result.Models.Should().OnlyContain(model =>
            !string.IsNullOrWhiteSpace(model.Id) &&
            !string.IsNullOrWhiteSpace(model.Name) &&
            !string.IsNullOrWhiteSpace(model.Provider));
    }

    private static readonly AgentThinkModel[] ExpectedFixtureModels =
    {
        new() { Id = "gpt-4o-mini", Name = "GPT-4o mini", Provider = "open_ai" },
        new() { Id = "claude-sonnet-4-5", Name = "Claude Sonnet 4.5", Provider = "anthropic" },
        new() { Id = "gemini-2.5-flash", Name = "Gemini 2.5 Flash", Provider = "google" },
        new() { Id = "openai/gpt-oss-20b", Name = "GPT OSS 20B", Provider = "groq" },
    };

    // Trimmed from a recorded GET https://agent.deepgram.com/v1/agent/settings/think/models response.
    private static string ReadThinkModelsFixture() => File.ReadAllText(
        Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "Agent", "think-models.json"));

    private sealed class ClearedCredentials : IDisposable
    {
        private readonly string? _apiKey = Environment.GetEnvironmentVariable(Deepgram.Constants.Defaults.DEEPGRAM_API_KEY);
        private readonly string? _accessToken = Environment.GetEnvironmentVariable(Deepgram.Constants.Defaults.DEEPGRAM_ACCESS_TOKEN);

        public ClearedCredentials()
        {
            Environment.SetEnvironmentVariable(Deepgram.Constants.Defaults.DEEPGRAM_API_KEY, null);
            Environment.SetEnvironmentVariable(Deepgram.Constants.Defaults.DEEPGRAM_ACCESS_TOKEN, null);
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable(Deepgram.Constants.Defaults.DEEPGRAM_API_KEY, _apiKey);
            Environment.SetEnvironmentVariable(Deepgram.Constants.Defaults.DEEPGRAM_ACCESS_TOKEN, _accessToken);
        }
    }

    private sealed class CapturingHttpMessageHandler(string body) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) });
        }
    }

    private sealed class RecordingLoggerProvider : ILoggerProvider
    {
        public ConcurrentQueue<(MelLogLevel Level, string Message)> Entries { get; } = new();

        public ILogger CreateLogger(string categoryName) => new RecordingLogger(this);

        public void Dispose() { }

        private sealed class RecordingLogger(RecordingLoggerProvider provider) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(MelLogLevel logLevel) => logLevel != MelLogLevel.None;

            public void Log<TState>(MelLogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter)
                => provider.Entries.Enqueue((logLevel, formatter(state, exception)));
        }
    }
}
