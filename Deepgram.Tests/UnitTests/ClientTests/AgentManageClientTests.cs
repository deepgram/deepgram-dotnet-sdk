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
    public async Task UpdateAgentMetadata_Should_Call_PutAllowingEmptyResponseAsync_Returning_AgentConfigurationResponse()
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

        // Mock methods. The client uses the empty-body-tolerant helper for this
        // operation (the live API answers this PUT with 200 and an empty body).
        agentManageClient.When(x => x.PutAllowingEmptyResponseAsync<AgentMetadataSchema, AgentConfigurationResponse>(
            Arg.Any<string>(), Arg.Any<AgentMetadataSchema>())).DoNotCallBase();
        agentManageClient.PutAllowingEmptyResponseAsync<AgentMetadataSchema, AgentConfigurationResponse>(url, metadataSchema).Returns(expectedResponse);

        // Act
        var result = await agentManageClient.UpdateAgentMetadata(_projectId, _agentId, metadataSchema);

        // Assert
        await agentManageClient.Received().PutAllowingEmptyResponseAsync<AgentMetadataSchema, AgentConfigurationResponse>(url, metadataSchema);
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<AgentConfigurationResponse>();
            result.Should().BeSameAs(expectedResponse);
        }
    }

    [Test]
    public async Task DeleteAgent_Should_Call_DeleteAllowingEmptyResponseAsync_Returning_DeleteResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.AGENTS}/{_agentId}");
        var expectedResponse = new DeleteResponse();

        // Fake Client
        var agentManageClient = Substitute.For<AgentManageClient>(_apiKey, _options, null);

        // Mock methods. The client uses the empty-body-tolerant helper for this
        // operation (the live API answers this DELETE with 200 and an empty body).
        agentManageClient.When(x => x.DeleteAllowingEmptyResponseAsync<DeleteResponse>(Arg.Any<string>())).DoNotCallBase();
        agentManageClient.DeleteAllowingEmptyResponseAsync<DeleteResponse>(url).Returns(expectedResponse);

        // Act
        var result = await agentManageClient.DeleteAgent(_projectId, _agentId);

        // Assert
        await agentManageClient.Received().DeleteAllowingEmptyResponseAsync<DeleteResponse>(url);
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
    public async Task UpdateAgentVariable_Should_Call_PatchAllowingEmptyResponseAsync_Returning_AgentVariableResponse()
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

        // Mock methods. The client uses the empty-body-tolerant helper for this
        // operation (the live API answers this PATCH with 200 and an empty body).
        agentManageClient.When(x => x.PatchAllowingEmptyResponseAsync<UpdateAgentVariableSchema, NoopSchema, AgentVariableResponse>(
            Arg.Any<string>(), Arg.Any<NoopSchema>(), Arg.Any<UpdateAgentVariableSchema>())).DoNotCallBase();
        agentManageClient.PatchAllowingEmptyResponseAsync<UpdateAgentVariableSchema, NoopSchema, AgentVariableResponse>(url, null, updateSchema)
            .Returns(expectedResponse);

        // Act
        var result = await agentManageClient.UpdateAgentVariable(_projectId, _variableId, updateSchema);

        // Assert
        await agentManageClient.Received().PatchAllowingEmptyResponseAsync<UpdateAgentVariableSchema, NoopSchema, AgentVariableResponse>(url, null, updateSchema);
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeAssignableTo<AgentVariableResponse>();
            result.Should().BeSameAs(expectedResponse);
        }
    }

    [Test]
    public async Task DeleteAgentVariable_Should_Call_DeleteAllowingEmptyResponseAsync_Returning_DeleteResponse()
    {
        // Input and Output
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.AGENT_VARIABLES}/{_variableId}");
        var expectedResponse = new DeleteResponse();

        // Fake Client
        var agentManageClient = Substitute.For<AgentManageClient>(_apiKey, _options, null);

        // Mock methods. The client uses the empty-body-tolerant helper for this
        // operation (the live API answers this DELETE with 200 and an empty body).
        agentManageClient.When(x => x.DeleteAllowingEmptyResponseAsync<DeleteResponse>(Arg.Any<string>())).DoNotCallBase();
        agentManageClient.DeleteAllowingEmptyResponseAsync<DeleteResponse>(url).Returns(expectedResponse);

        // Act
        var result = await agentManageClient.DeleteAgentVariable(_projectId, _variableId);

        // Assert
        await agentManageClient.Received().DeleteAllowingEmptyResponseAsync<DeleteResponse>(url);
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
            "config": { "language": "en" },
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
            response.Metadata!["name"].Should().Be("support-agent");
            response.CreatedAt.Should().Be(new DateTime(2026, 8, 28, 12, 34, 56, DateTimeKind.Utc));
            response.UpdatedAt.Should().Be(new DateTime(2026, 8, 29, 1, 2, 3, DateTimeKind.Utc));
        }
    }

    [Test]
    public void Uninterpolated_Config_Should_Preserve_Bare_Template_Variable_Tokens()
    {
        // The documented template-variable syntax is a bare, unquoted DG_<VARIABLE_NAME> token
        // inside the JSON-encoded config (e.g. "greeting": DG_GREETING — no quotes, no braces).
        // The live API stores and returns the config as that JSON-encoded STRING, so the
        // uninterpolated response must hand the bare token back exactly as written.
        var json = """
        {
            "agent_uuid": "9d4b6c9e-0000-0000-0000-000000000000",
            "config": "{ \"language\": \"en\", \"greeting\": DG_GREETING }",
            "metadata": { "name": "support-agent" }
        }
        """;

        var response = JsonSerializer.Deserialize<AgentConfigurationResponse>(json);

        using (new AssertionScope())
        {
            response!.Config!.Value.ValueKind.Should().Be(JsonValueKind.String);
            var storedConfig = response.Config!.Value.GetString();
            storedConfig.Should().Contain("\"greeting\": DG_GREETING",
                "the bare token must come back uninterpolated, unquoted, and unbraced");
            storedConfig.Should().NotContain("{{",
                "brace-wrapped placeholders are not a supported template variable syntax");
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
    public void AgentConfigurations_Should_Deserialize_From_Live_Bare_Array_Shape()
    {
        // Captured from the live API 2026-08-31: a bare array (not {"agents":[...]}), ids as
        // agent_uuid (not agent_id), and config as the stored JSON-encoded STRING (not a
        // parsed object). The SDK accepts this shape alongside the documented one.
        var json = """
        [{
            "agent_uuid": "8f153566-fd4b-4ad4-bc13-09c66e0eed64",
            "member_id": "43d2a494-32e3-40ba-9c6e-6ab030597ead",
            "api_version": 1,
            "config": "{\"language\":\"en\"}",
            "metadata": { "name": "customer-service-agent" }
        }]
        """;

        var response = JsonSerializer.Deserialize<AgentConfigurationsResponse>(json);

        using (new AssertionScope())
        {
            response!.Agents.Should().ContainSingle();
            var agent = response.Agents![0];
            agent.AgentUuid.Should().Be("8f153566-fd4b-4ad4-bc13-09c66e0eed64");
            // AgentId falls back to the live agent_uuid field.
            agent.AgentId.Should().Be("8f153566-fd4b-4ad4-bc13-09c66e0eed64");
            agent.ApiVersion.Should().Be(1);
            agent.Config!.Value.ValueKind.Should().Be(JsonValueKind.String);
            agent.Metadata!["name"].Should().Be("customer-service-agent");
        }
    }

    [Test]
    public void AgentConfigurations_Should_Deserialize_From_Documented_Object_Shape()
    {
        var json = """{ "agents": [ { "agent_id": "abc", "config": {} } ] }""";

        var response = JsonSerializer.Deserialize<AgentConfigurationsResponse>(json);

        response!.Agents.Should().ContainSingle().Which.AgentId.Should().Be("abc");
    }

    [Test]
    public void AgentVariables_Should_Deserialize_From_Live_Bare_Array_Shape()
    {
        // Captured from the live API 2026-08-31: bare array, ids as agent_variable_uuid
        // (not variable_id), plus is_sensitive.
        var json = """
        [{
            "agent_variable_uuid": "ec490d38-3cfc-4452-8a75-40dd14e69a7e",
            "member_id": "43d2a494-32e3-40ba-9c6e-6ab030597ead",
            "api_version": 1,
            "key": "DG_SMOKE_TEST",
            "value": "hello",
            "is_sensitive": false
        }]
        """;

        var response = JsonSerializer.Deserialize<AgentVariablesResponse>(json);

        using (new AssertionScope())
        {
            response!.Variables.Should().ContainSingle();
            var variable = response.Variables![0];
            variable.AgentVariableUuid.Should().Be("ec490d38-3cfc-4452-8a75-40dd14e69a7e");
            // VariableId falls back to the live agent_variable_uuid field.
            variable.VariableId.Should().Be("ec490d38-3cfc-4452-8a75-40dd14e69a7e");
            variable.Key.Should().Be("DG_SMOKE_TEST");
            variable.Value!.Value.GetString().Should().Be("hello");
            variable.IsSensitive.Should().BeFalse();
        }
    }

    [Test]
    public void CreateAgent_Response_Should_Map_Live_AgentUuid_To_AgentId()
    {
        // Captured from the live API 2026-08-31: POST /agents returns only the uuid.
        var response = JsonSerializer.Deserialize<AgentConfigurationResponse>(
            """{ "agent_uuid": "28f134a8-0967-45cb-a792-8cf8f729e586" }""");

        response!.AgentId.Should().Be("28f134a8-0967-45cb-a792-8cf8f729e586");
    }

    [Test]
    public void Documented_Id_Should_Win_Over_Live_Uuid_When_Both_Present()
    {
        var agent = JsonSerializer.Deserialize<AgentConfigurationResponse>(
            """{ "agent_id": "documented", "agent_uuid": "live" }""");
        var variable = JsonSerializer.Deserialize<AgentVariableResponse>(
            """{ "variable_id": "documented", "agent_variable_uuid": "live" }""");

        using (new AssertionScope())
        {
            agent!.AgentId.Should().Be("documented");
            variable!.VariableId.Should().Be("documented");
        }
    }

    [Test]
    public void AgentVariableSchema_Should_Serialize_IsSensitive_False_By_Default()
    {
        // The live API requires is_sensitive and currently only accepts false; omitting it
        // rejects the request with "Sensitive template variables are not supported yet".
        var schema = new AgentVariableSchema { Key = "DG_X", Value = "y" };

        using var doc = JsonDocument.Parse(schema.ToString());

        doc.RootElement.GetProperty("is_sensitive").GetBoolean().Should().BeFalse();
    }

    [Test]
    public async Task UpdateAgentMetadata_Should_Return_Empty_Response_When_Body_Is_Empty()
    {
        // The live API answers PUT /agents/{id} with 200 and an empty body; the client must
        // hand back an (empty) response object, not null and not an exception.
        var url = AbstractRestClient.GetUri(_options, $"{UriSegments.PROJECTS}/{_projectId}/{UriSegments.AGENTS}/{_agentId}");
        var metadataSchema = new AgentMetadataSchema { Metadata = new Dictionary<string, string> { ["a"] = "b" } };

        var agentManageClient = Substitute.For<AgentManageClient>(_apiKey, _options, null);
        agentManageClient.When(x => x.PutAllowingEmptyResponseAsync<AgentMetadataSchema, AgentConfigurationResponse>(
            Arg.Any<string>(), Arg.Any<AgentMetadataSchema>())).DoNotCallBase();
        agentManageClient.PutAllowingEmptyResponseAsync<AgentMetadataSchema, AgentConfigurationResponse>(url, metadataSchema)
            .Returns((AgentConfigurationResponse?)null!);

        var result = await agentManageClient.UpdateAgentMetadata(_projectId, _agentId, metadataSchema);

        result.Should().NotBeNull();
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

    // The list converters accept exactly the two known shapes (documented envelope, captured
    // bare array). Every other successful-looking payload must throw JsonException so API
    // contract drift is visible instead of silently becoming "a project with no resources".
    [TestCase("""{ "items": [ { "agent_id": "abc" } ] }""", TestName = "AgentConfigurations_Unknown_Envelope_Should_Throw")]
    [TestCase("""{ "agents": "not-an-array" }""", TestName = "AgentConfigurations_Wrong_Property_Type_Should_Throw")]
    [TestCase("""{ "agents": null }""", TestName = "AgentConfigurations_Null_Property_Should_Throw")]
    [TestCase("""{}""", TestName = "AgentConfigurations_Empty_Object_Should_Throw")]
    [TestCase(""" "scalar" """, TestName = "AgentConfigurations_Scalar_Should_Throw")]
    [TestCase("42", TestName = "AgentConfigurations_Number_Should_Throw")]
    [TestCase("null", TestName = "AgentConfigurations_Null_Should_Throw")]
    public void AgentConfigurations_Should_Throw_On_Unsupported_List_Shapes(string json)
    {
        var act = () => JsonSerializer.Deserialize<AgentConfigurationsResponse>(json);

        act.Should().Throw<JsonException>();
    }

    [TestCase("""{ "items": [ { "variable_id": "abc" } ] }""", TestName = "AgentVariables_Unknown_Envelope_Should_Throw")]
    [TestCase("""{ "variables": "not-an-array" }""", TestName = "AgentVariables_Wrong_Property_Type_Should_Throw")]
    [TestCase("""{ "variables": null }""", TestName = "AgentVariables_Null_Property_Should_Throw")]
    [TestCase("""{}""", TestName = "AgentVariables_Empty_Object_Should_Throw")]
    [TestCase(""" "scalar" """, TestName = "AgentVariables_Scalar_Should_Throw")]
    [TestCase("42", TestName = "AgentVariables_Number_Should_Throw")]
    [TestCase("null", TestName = "AgentVariables_Null_Should_Throw")]
    public void AgentVariables_Should_Throw_On_Unsupported_List_Shapes(string json)
    {
        var act = () => JsonSerializer.Deserialize<AgentVariablesResponse>(json);

        act.Should().Throw<JsonException>();
    }
    #endregion

    #region Empty-body transport contract
    // These run the REAL request/deserialization pipeline (no NSubstitute) against a stubbed
    // HTTP transport, proving the empty-body opt-in is scoped to exactly the four Agent
    // management operations whose live success responses are empty — and that everything else
    // keeps the fail-fast contract.
    private static T WithEmptyBodyTransport<T>(T client) where T : AbstractRestClient
    {
        client._httpClient = MockHttpClient.CreateHttpClientWithRawResult("", HttpStatusCode.OK);
        return client;
    }

    [Test]
    public async Task UpdateAgentMetadata_Should_Accept_Empty_Success_Body_Over_Transport()
    {
        var client = WithEmptyBodyTransport(new AgentManageClient(_apiKey, _options));
        var metadataSchema = new AgentMetadataSchema { Metadata = new Dictionary<string, string> { ["a"] = "b" } };

        var result = await client.UpdateAgentMetadata(_projectId, _agentId, metadataSchema);

        result.Should().NotBeNull();
    }

    [Test]
    public async Task UpdateAgentVariable_Should_Accept_Empty_Success_Body_Over_Transport()
    {
        var client = WithEmptyBodyTransport(new AgentManageClient(_apiKey, _options));

        var result = await client.UpdateAgentVariable(_projectId, _variableId, new UpdateAgentVariableSchema { Value = "x" });

        result.Should().NotBeNull();
    }

    [Test]
    public async Task DeleteAgent_Should_Accept_Empty_Success_Body_Over_Transport()
    {
        var client = WithEmptyBodyTransport(new AgentManageClient(_apiKey, _options));

        var result = await client.DeleteAgent(_projectId, _agentId);

        result.Should().NotBeNull();
    }

    [Test]
    public async Task DeleteAgentVariable_Should_Accept_Empty_Success_Body_Over_Transport()
    {
        var client = WithEmptyBodyTransport(new AgentManageClient(_apiKey, _options));

        var result = await client.DeleteAgentVariable(_projectId, _variableId);

        result.Should().NotBeNull();
    }

    [Test]
    public async Task Transcription_Should_Throw_On_Empty_Success_Body_Over_Transport()
    {
        // A required-body endpoint: a truncated 200 with no JSON must fail at the transport
        // boundary, never surface as a successful null result.
        var client = WithEmptyBodyTransport(new Deepgram.Clients.Listen.v1.REST.Client(_apiKey, _options));

        var act = async () => await client.TranscribeUrl(
            new Deepgram.Models.Listen.v1.REST.UrlSource("https://dpgr.am/bueller.wav"),
            new Deepgram.Models.Listen.v1.REST.PreRecordedSchema { Model = "nova-3" });

        await act.Should().ThrowAsync<JsonException>();
    }

    [Test]
    public async Task Agent_Read_Operations_Should_Throw_On_Empty_Success_Body_Over_Transport()
    {
        // Only the update/delete operations opt in: the Agent GET/LIST/CREATE contracts
        // require JSON, so an empty 200 still fails fast even inside this client.
        var client = WithEmptyBodyTransport(new AgentManageClient(_apiKey, _options));

        var actGet = async () => await client.GetAgent(_projectId, _agentId);
        var actList = async () => await client.GetAgents(_projectId);

        await actGet.Should().ThrowAsync<JsonException>();
        await actList.Should().ThrowAsync<JsonException>();
    }
    #endregion
}
