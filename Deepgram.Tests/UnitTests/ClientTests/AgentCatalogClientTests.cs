// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Clients.Agent.v1.REST;
using Deepgram.Models.Agent.v1.REST;
using Deepgram.Models.Authenticate.v1;
using AgentCatalogRestClient = Deepgram.Clients.Agent.v1.REST.Client;

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
}
