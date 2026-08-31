// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.AgentManage.v1;
using Deepgram.Models.Exceptions.v1;
using Deepgram.Clients.AgentManage.v1;
using Deepgram.Abstractions.v1;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class AgentManageClientTests
{
    DeepgramHttpClientOptions _options;
    string _projectId;
    string _agentId;
    string _variableId;
    string _apiKey;

    [SetUp]
    public void Setup()
    {
        _apiKey = new Faker().Random.Guid().ToString();
        _options = new DeepgramHttpClientOptions(_apiKey)
        {
            OnPrem = true,
        };
        _projectId = new Faker().Random.Guid().ToString();
        _agentId = new Faker().Random.Guid().ToString();
        _variableId = new Faker().Random.Guid().ToString();
    }

    // Built by hand rather than with AutoFaker: these responses carry JsonElement members,
    // and AutoFaker generates default (Undefined) JsonElements that throw on serialization.
    private static AgentConfigurationResponse BuildAgentConfiguration(string agentId) => new()
    {
        AgentId = agentId,
        Config = JsonSerializer.SerializeToElement(new { language = "en" }),
        Metadata = new Dictionary<string, string> { ["name"] = "support-agent" },
        CreatedAt = new DateTime(2026, 8, 28, 12, 34, 56, DateTimeKind.Utc),
        UpdatedAt = new DateTime(2026, 8, 29, 1, 2, 3, DateTimeKind.Utc),
    };

    private static AgentVariableResponse BuildAgentVariable(string variableId) => new()
    {
        VariableId = variableId,
        Key = "DG_GREETING",
        Value = JsonSerializer.SerializeToElement("Hello! How can I help you today?"),
        CreatedAt = new DateTime(2026, 8, 28, 12, 34, 56, DateTimeKind.Utc),
        UpdatedAt = new DateTime(2026, 8, 28, 12, 34, 56, DateTimeKind.Utc),
    };

    #region Agent Configurations
    [Test]
    public async Task GetAgents_Should_Call_GetAsync_Returning_AgentConfigurationsResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.AGENTS}");
        var expectedResponse = new AgentConfigurationsResponse { Agents = new List<AgentConfigurationResponse> { BuildAgentConfiguration(_agentId) } };

        // Fake Client
        var agentManageClient = Substitute.For<AgentManageClient>(_apiKey, _options, null);

        // Mock methods
        agentManageClient.When(x => x.GetAsync<AgentConfigurationsResponse>(Arg.Any<string>())).DoNotCallBase();
        agentManageClient.GetAsync<AgentConfigurationsResponse>(url).Returns(expectedResponse);

        // Act
        var result = await agentManageClient.GetAgents(_projectId);

        // Assert
        await agentManageClient.Received().GetAsync<AgentConfigurationsResponse>(url);
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<AgentConfigurationsResponse>();
            result.Should().BeSameAs(expectedResponse);
        }
    }

    [Test]
    public async Task GetAgent_Should_Call_GetAsync_Returning_AgentConfigurationResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.AGENTS}/{_agentId}");
        var expectedResponse = BuildAgentConfiguration(_agentId);

        // Fake Client
        var agentManageClient = Substitute.For<AgentManageClient>(_apiKey, _options, null);

        // Mock methods
        agentManageClient.When(x => x.GetAsync<AgentConfigurationResponse>(Arg.Any<string>())).DoNotCallBase();
        agentManageClient.GetAsync<AgentConfigurationResponse>(url).Returns(expectedResponse);

        // Act
        var result = await agentManageClient.GetAgent(_projectId, _agentId);

        // Assert
        await agentManageClient.Received().GetAsync<AgentConfigurationResponse>(url);
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<AgentConfigurationResponse>();
            result.Should().BeSameAs(expectedResponse);
        }
    }

    [Test]
    public async Task CreateAgent_Should_Call_PostAsync_Returning_AgentConfigurationResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.AGENTS}");
        var expectedResponse = BuildAgentConfiguration(_agentId);
        var configurationSchema = new AgentConfigurationSchema
        {
            Config = """{"language":"en"}""",
            Metadata = new Dictionary<string, string> { ["name"] = "customer-service-agent" },
        };

        // Fake Client
        var agentManageClient = Substitute.For<AgentManageClient>(_apiKey, _options, null);

        // Mock methods
        agentManageClient.When(x => x.PostAsync<AgentConfigurationSchema, NoopSchema, AgentConfigurationResponse>(
            Arg.Any<string>(), Arg.Any<NoopSchema>(), Arg.Any<AgentConfigurationSchema>())).DoNotCallBase();
        agentManageClient.PostAsync<AgentConfigurationSchema, NoopSchema, AgentConfigurationResponse>(url, null, configurationSchema)
            .Returns(expectedResponse);

        // Act
        var result = await agentManageClient.CreateAgent(_projectId, configurationSchema);

        // Assert
        await agentManageClient.Received().PostAsync<AgentConfigurationSchema, NoopSchema, AgentConfigurationResponse>(url, null, configurationSchema);
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<AgentConfigurationResponse>();
            result.Should().BeSameAs(expectedResponse);
        }
    }

    [Test]
    public void CreateAgent_Without_Config_Should_Throw()
    {
        var agentManageClient = new AgentManageClient(_apiKey, _options);

        Assert.ThrowsAsync<ArgumentNullException>(async () => await agentManageClient.CreateAgent(_projectId, null!));
        Assert.ThrowsAsync<DeepgramException>(async () => await agentManageClient.CreateAgent(_projectId, new AgentConfigurationSchema()));
    }

    [Test]
    public async Task UpdateAgentMetadata_Should_Call_PutAsync_Returning_AgentConfigurationResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.AGENTS}/{_agentId}");
        var expectedResponse = BuildAgentConfiguration(_agentId);
        var metadataSchema = new AgentMetadataSchema
        {
            Metadata = new Dictionary<string, string> { ["environment"] = "production" },
        };

        // Fake Client
        var agentManageClient = Substitute.For<AgentManageClient>(_apiKey, _options, null);

        // Mock methods
        agentManageClient.When(x => x.PutAsync<AgentMetadataSchema, AgentConfigurationResponse>(
            Arg.Any<string>(), Arg.Any<AgentMetadataSchema>())).DoNotCallBase();
        agentManageClient.PutAsync<AgentMetadataSchema, AgentConfigurationResponse>(url, metadataSchema).Returns(expectedResponse);

        // Act
        var result = await agentManageClient.UpdateAgentMetadata(_projectId, _agentId, metadataSchema);

        // Assert
        await agentManageClient.Received().PutAsync<AgentMetadataSchema, AgentConfigurationResponse>(url, metadataSchema);
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<AgentConfigurationResponse>();
            result.Should().BeSameAs(expectedResponse);
        }
    }

    [Test]
    public async Task DeleteAgent_Should_Call_DeleteAsync_Returning_DeleteResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.AGENTS}/{_agentId}");
        var expectedResponse = new DeleteResponse();

        // Fake Client
        var agentManageClient = Substitute.For<AgentManageClient>(_apiKey, _options, null);

        // Mock methods
        agentManageClient.When(x => x.DeleteAsync<DeleteResponse>(Arg.Any<string>())).DoNotCallBase();
        agentManageClient.DeleteAsync<DeleteResponse>(url).Returns(expectedResponse);

        // Act
        var result = await agentManageClient.DeleteAgent(_projectId, _agentId);

        // Assert
        await agentManageClient.Received().DeleteAsync<DeleteResponse>(url);
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<DeleteResponse>();
        }
    }
    #endregion

    #region Agent Variables
    [Test]
    public async Task GetAgentVariables_Should_Call_GetAsync_Returning_AgentVariablesResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.AGENT_VARIABLES}");
        var expectedResponse = new AgentVariablesResponse { Variables = new List<AgentVariableResponse> { BuildAgentVariable(_variableId) } };

        // Fake Client
        var agentManageClient = Substitute.For<AgentManageClient>(_apiKey, _options, null);

        // Mock methods
        agentManageClient.When(x => x.GetAsync<AgentVariablesResponse>(Arg.Any<string>())).DoNotCallBase();
        agentManageClient.GetAsync<AgentVariablesResponse>(url).Returns(expectedResponse);

        // Act
        var result = await agentManageClient.GetAgentVariables(_projectId);

        // Assert
        await agentManageClient.Received().GetAsync<AgentVariablesResponse>(url);
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<AgentVariablesResponse>();
            result.Should().BeSameAs(expectedResponse);
        }
    }

    [Test]
    public async Task GetAgentVariable_Should_Call_GetAsync_Returning_AgentVariableResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.AGENT_VARIABLES}/{_variableId}");
        var expectedResponse = BuildAgentVariable(_variableId);

        // Fake Client
        var agentManageClient = Substitute.For<AgentManageClient>(_apiKey, _options, null);

        // Mock methods
        agentManageClient.When(x => x.GetAsync<AgentVariableResponse>(Arg.Any<string>())).DoNotCallBase();
        agentManageClient.GetAsync<AgentVariableResponse>(url).Returns(expectedResponse);

        // Act
        var result = await agentManageClient.GetAgentVariable(_projectId, _variableId);

        // Assert
        await agentManageClient.Received().GetAsync<AgentVariableResponse>(url);
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<AgentVariableResponse>();
            result.Should().BeSameAs(expectedResponse);
        }
    }

    [Test]
    public async Task CreateAgentVariable_Should_Call_PostAsync_Returning_AgentVariableResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.AGENT_VARIABLES}");
        var expectedResponse = BuildAgentVariable(_variableId);
        var variableSchema = new AgentVariableSchema
        {
            Key = "DG_GREETING",
            Value = "Hello! How can I help you today?",
        };

        // Fake Client
        var agentManageClient = Substitute.For<AgentManageClient>(_apiKey, _options, null);

        // Mock methods
        agentManageClient.When(x => x.PostAsync<AgentVariableSchema, NoopSchema, AgentVariableResponse>(
            Arg.Any<string>(), Arg.Any<NoopSchema>(), Arg.Any<AgentVariableSchema>())).DoNotCallBase();
        agentManageClient.PostAsync<AgentVariableSchema, NoopSchema, AgentVariableResponse>(url, null, variableSchema)
            .Returns(expectedResponse);

        // Act
        var result = await agentManageClient.CreateAgentVariable(_projectId, variableSchema);

        // Assert
        await agentManageClient.Received().PostAsync<AgentVariableSchema, NoopSchema, AgentVariableResponse>(url, null, variableSchema);
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<AgentVariableResponse>();
            result.Should().BeSameAs(expectedResponse);
        }
    }

    [Test]
    public void CreateAgentVariable_Without_Key_Or_Value_Should_Throw()
    {
        var agentManageClient = new AgentManageClient(_apiKey, _options);

        Assert.ThrowsAsync<ArgumentNullException>(async () => await agentManageClient.CreateAgentVariable(_projectId, null!));
        Assert.ThrowsAsync<DeepgramException>(async () =>
            await agentManageClient.CreateAgentVariable(_projectId, new AgentVariableSchema { Value = "x" }));
        Assert.ThrowsAsync<DeepgramException>(async () =>
            await agentManageClient.CreateAgentVariable(_projectId, new AgentVariableSchema { Key = "DG_X" }));
    }

    [Test]
    public async Task UpdateAgentVariable_Should_Call_PatchAsync_Returning_AgentVariableResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.AGENT_VARIABLES}/{_variableId}");
        var expectedResponse = BuildAgentVariable(_variableId);
        var updateSchema = new UpdateAgentVariableSchema
        {
            Value = "Welcome back!",
        };

        // Fake Client
        var agentManageClient = Substitute.For<AgentManageClient>(_apiKey, _options, null);

        // Mock methods
        agentManageClient.When(x => x.PatchAsync<UpdateAgentVariableSchema, NoopSchema, AgentVariableResponse>(
            Arg.Any<string>(), Arg.Any<NoopSchema>(), Arg.Any<UpdateAgentVariableSchema>())).DoNotCallBase();
        agentManageClient.PatchAsync<UpdateAgentVariableSchema, NoopSchema, AgentVariableResponse>(url, null, updateSchema)
            .Returns(expectedResponse);

        // Act
        var result = await agentManageClient.UpdateAgentVariable(_projectId, _variableId, updateSchema);

        // Assert
        await agentManageClient.Received().PatchAsync<UpdateAgentVariableSchema, NoopSchema, AgentVariableResponse>(url, null, updateSchema);
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<AgentVariableResponse>();
            result.Should().BeSameAs(expectedResponse);
        }
    }

    [Test]
    public async Task DeleteAgentVariable_Should_Call_DeleteAsync_Returning_DeleteResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.AGENT_VARIABLES}/{_variableId}");
        var expectedResponse = new DeleteResponse();

        // Fake Client
        var agentManageClient = Substitute.For<AgentManageClient>(_apiKey, _options, null);

        // Mock methods
        agentManageClient.When(x => x.DeleteAsync<DeleteResponse>(Arg.Any<string>())).DoNotCallBase();
        agentManageClient.DeleteAsync<DeleteResponse>(url).Returns(expectedResponse);

        // Act
        var result = await agentManageClient.DeleteAgentVariable(_projectId, _variableId);

        // Assert
        await agentManageClient.Received().DeleteAsync<DeleteResponse>(url);
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<DeleteResponse>();
        }
    }
    #endregion

    #region Wire shapes
    [Test]
    public void AgentConfigurationSchema_Should_Serialize_To_API_Shape()
    {
        // The create request carries config as a JSON-encoded STRING, not a nested object.
        var schema = new AgentConfigurationSchema
        {
            Config = """{"language":"en"}""",
            Metadata = new Dictionary<string, string> { ["name"] = "support-agent" },
        };

        using var doc = JsonDocument.Parse(schema.ToString());

        doc.RootElement.GetProperty("config").ValueKind.Should().Be(JsonValueKind.String);
        doc.RootElement.GetProperty("config").GetString().Should().Be("""{"language":"en"}""");
        doc.RootElement.GetProperty("metadata").GetProperty("name").GetString().Should().Be("support-agent");
        doc.RootElement.TryGetProperty("api_version", out _).Should().BeFalse();
    }

    [Test]
    public void AgentVariableSchema_Should_Serialize_Any_Json_Value()
    {
        // A variable value can be any valid JSON type, including an object.
        var schema = new AgentVariableSchema
        {
            Key = "DG_MODEL_CONFIG",
            Value = new Dictionary<string, object> { ["temperature"] = 0.5 },
        };

        using var doc = JsonDocument.Parse(schema.ToString());

        doc.RootElement.GetProperty("key").GetString().Should().Be("DG_MODEL_CONFIG");
        doc.RootElement.GetProperty("value").GetProperty("temperature").GetDouble().Should().Be(0.5);
    }

    [Test]
    public void UpdateAgentVariableSchema_Should_Serialize_Value_Only()
    {
        var schema = new UpdateAgentVariableSchema { Value = "new value" };

        using var doc = JsonDocument.Parse(schema.ToString());

        doc.RootElement.GetProperty("value").GetString().Should().Be("new value");
        doc.RootElement.EnumerateObject().Count().Should().Be(1);
    }

    [Test]
    public void AgentConfigurationResponse_Should_Deserialize_From_API_Shape()
    {
        // Response shape per the Deepgram OpenAPI spec: config comes back as a parsed OBJECT.
        var json = """
        {
            "agent_id": "9d4b6c9e-0000-0000-0000-000000000000",
            "config": { "language": "en", "greeting": "{{DG_GREETING}}" },
            "metadata": { "name": "support-agent" },
            "created_at": "2026-08-28T12:34:56Z",
            "updated_at": "2026-08-29T01:02:03Z"
        }
        """;

        var response = JsonSerializer.Deserialize<AgentConfigurationResponse>(json);

        using (new AssertionScope())
        {
            response!.AgentId.Should().Be("9d4b6c9e-0000-0000-0000-000000000000");
            response.Config!.Value.GetProperty("language").GetString().Should().Be("en");
            // Uninterpolated form: template variable placeholders come back as-is.
            response.Config!.Value.GetProperty("greeting").GetString().Should().Be("{{DG_GREETING}}");
            response.Metadata!["name"].Should().Be("support-agent");
            response.CreatedAt.Should().Be(new DateTime(2026, 8, 28, 12, 34, 56, DateTimeKind.Utc));
            response.UpdatedAt.Should().Be(new DateTime(2026, 8, 29, 1, 2, 3, DateTimeKind.Utc));
        }
    }

    [Test]
    public void AgentVariableResponse_Should_Deserialize_From_API_Shape()
    {
        var json = """
        {
            "variables": [
                {
                    "variable_id": "1e2f3a4b-0000-0000-0000-000000000000",
                    "key": "DG_GREETING",
                    "value": "Hello! How can I help you today?",
                    "created_at": "2026-08-28T12:34:56Z",
                    "updated_at": "2026-08-28T12:34:56Z"
                },
                {
                    "variable_id": "2e2f3a4b-0000-0000-0000-000000000000",
                    "key": "DG_MODEL_CONFIG",
                    "value": { "temperature": 0.5 }
                }
            ]
        }
        """;

        var response = JsonSerializer.Deserialize<AgentVariablesResponse>(json);

        using (new AssertionScope())
        {
            response!.Variables.Should().HaveCount(2);
            response.Variables![0].Key.Should().Be("DG_GREETING");
            response.Variables[0].Value!.Value.GetString().Should().Be("Hello! How can I help you today?");
            // Values can be any JSON type, including objects.
            response.Variables[1].Value!.Value.GetProperty("temperature").GetDouble().Should().Be(0.5);
        }
    }

    [Test]
    public void DeleteResponse_Should_Deserialize_From_Empty_And_NonEmpty_Objects()
    {
        var empty = JsonSerializer.Deserialize<DeleteResponse>("{}");
        var withMessage = JsonSerializer.Deserialize<DeleteResponse>("""{ "message": "deleted" }""");

        using (new AssertionScope())
        {
            empty.Should().NotBeNull();
            withMessage!.AdditionalProperties!["message"].GetString().Should().Be("deleted");
        }
    }
    #endregion
}
