// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using NSubstitute;
using System.Text.Json;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Agent.v2.WebSocket;
using Deepgram.Clients.Agent.v2.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class AgentHistoryTests
{
    DeepgramWsClientOptions _options;
    string _apiKey;

    [SetUp]
    public void Setup()
    {
        _apiKey = new Faker().Random.Guid().ToString();
        _options = new DeepgramWsClientOptions(_apiKey)
        {
            OnPrem = true,
        };
    }

    #region SendHistoryConversationText Tests

    [Test]
    public async Task SendHistoryConversationText_With_String_Parameters_Should_Send_Message()
    {
        // Input and Output
        var role = "user";
        var content = "What's the weather like today?";
        var agentClient = Substitute.For<Client>(_apiKey, _options);

        // Mock the SendMessageImmediately method
        agentClient.When(x => x.SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<CancellationTokenSource>()))
                   .DoNotCallBase();

        // Act
        await agentClient.SendHistoryConversationText(role, content);

        // Assert
        await agentClient.Received(1).SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<CancellationTokenSource>());
    }

    [Test]
    public async Task SendHistoryConversationText_With_Schema_Should_Send_Message()
    {
        // Input and Output
        var schema = new HistoryConversationText
        {
            Role = "assistant",
            Content = "Based on the current data, it's sunny with a temperature of 72°F."
        };
        var agentClient = Substitute.For<Client>(_apiKey, _options);

        // Mock the SendMessageImmediately method
        agentClient.When(x => x.SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<CancellationTokenSource>()))
                   .DoNotCallBase();

        // Act
        await agentClient.SendHistoryConversationText(schema);

        // Assert
        await agentClient.Received(1).SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<CancellationTokenSource>());
    }

    [Test]
    public async Task SendHistoryConversationText_With_Null_Role_Should_Throw_ArgumentException()
    {
        // Input and Output
        string? role = null;
        var content = "Test content";
        var agentClient = new Client(_apiKey, _options);

        // Act & Assert
        var exception = await agentClient.Invoking(y => y.SendHistoryConversationText(role!, content))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Role cannot be null or empty*");
        exception.And.ParamName.Should().Be("role");
    }

    [Test]
    public async Task SendHistoryConversationText_With_Empty_Role_Should_Throw_ArgumentException()
    {
        // Input and Output
        var role = "";
        var content = "Test content";
        var agentClient = new Client(_apiKey, _options);

        // Act & Assert
        var exception = await agentClient.Invoking(y => y.SendHistoryConversationText(role, content))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Role cannot be null or empty*");
        exception.And.ParamName.Should().Be("role");
    }

    [Test]
    public async Task SendHistoryConversationText_With_Null_Content_Should_Throw_ArgumentException()
    {
        // Input and Output
        var role = "user";
        string? content = null;
        var agentClient = new Client(_apiKey, _options);

        // Act & Assert
        var exception = await agentClient.Invoking(y => y.SendHistoryConversationText(role, content!))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Content cannot be null or empty*");
        exception.And.ParamName.Should().Be("content");
    }

    [Test]
    public async Task SendHistoryConversationText_With_Null_Schema_Should_Throw_ArgumentNullException()
    {
        // Input and Output
        HistoryConversationText? schema = null;
        var agentClient = new Client(_apiKey, _options);

        // Act & Assert
        var exception = await agentClient.Invoking(y => y.SendHistoryConversationText(schema!))
            .Should().ThrowAsync<ArgumentNullException>();
        exception.And.ParamName.Should().Be("historyConversationText");
    }

    #endregion

    #region SendHistoryFunctionCalls Tests

    [Test]
    public async Task SendHistoryFunctionCalls_With_List_Should_Send_Message()
    {
        // Input and Output
        var functionCalls = new List<HistoryFunctionCall>
        {
            new HistoryFunctionCall
            {
                Id = "fc_12345678-90ab-cdef-1234-567890abcdef",
                Name = "check_order_status",
                ClientSide = true,
                Arguments = "{\"order_id\": \"ORD-123456\"}",
                Response = "Order #123456 status: Shipped - Expected delivery date: 2024-03-15"
            }
        };
        var agentClient = Substitute.For<Client>(_apiKey, _options);

        // Mock the SendMessageImmediately method
        agentClient.When(x => x.SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<CancellationTokenSource>()))
                   .DoNotCallBase();

        // Act
        await agentClient.SendHistoryFunctionCalls(functionCalls);

        // Assert
        await agentClient.Received(1).SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<CancellationTokenSource>());
    }

    [Test]
    public async Task SendHistoryFunctionCalls_With_Schema_Should_Send_Message()
    {
        // Input and Output
        var schema = new HistoryFunctionCalls
        {
            FunctionCalls = new List<HistoryFunctionCall>
            {
                new HistoryFunctionCall
                {
                    Id = "fc_12345678-90ab-cdef-1234-567890abcdef",
                    Name = "get_weather",
                    ClientSide = false,
                    Arguments = "{\"location\": \"New York\"}",
                    Response = "Temperature: 22°C, Conditions: Sunny"
                }
            }
        };
        var agentClient = Substitute.For<Client>(_apiKey, _options);

        // Mock the SendMessageImmediately method
        agentClient.When(x => x.SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<CancellationTokenSource>()))
                   .DoNotCallBase();

        // Act
        await agentClient.SendHistoryFunctionCalls(schema);

        // Assert
        await agentClient.Received(1).SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<CancellationTokenSource>());
    }

    [Test]
    public async Task SendHistoryFunctionCalls_With_Null_List_Should_Throw_ArgumentException()
    {
        // Input and Output
        List<HistoryFunctionCall>? functionCalls = null;
        var agentClient = new Client(_apiKey, _options);

        // Act & Assert
        var exception = await agentClient.Invoking(y => y.SendHistoryFunctionCalls(functionCalls!))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("FunctionCalls cannot be null or empty*");
        exception.And.ParamName.Should().Be("functionCalls");
    }

    [Test]
    public async Task SendHistoryFunctionCalls_With_Empty_List_Should_Throw_ArgumentException()
    {
        // Input and Output
        var functionCalls = new List<HistoryFunctionCall>();
        var agentClient = new Client(_apiKey, _options);

        // Act & Assert
        var exception = await agentClient.Invoking(y => y.SendHistoryFunctionCalls(functionCalls))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("FunctionCalls cannot be null or empty*");
        exception.And.ParamName.Should().Be("functionCalls");
    }

    [Test]
    public async Task SendHistoryFunctionCalls_With_Null_Schema_Should_Throw_ArgumentNullException()
    {
        // Input and Output
        HistoryFunctionCalls? schema = null;
        var agentClient = new Client(_apiKey, _options);

        // Act & Assert
        var exception = await agentClient.Invoking(y => y.SendHistoryFunctionCalls(schema!))
            .Should().ThrowAsync<ArgumentNullException>();
        exception.And.ParamName.Should().Be("historyFunctionCalls");
    }

    #endregion

    #region Model Serialization Tests

    [Test]
    public void HistoryConversationText_Should_Have_Correct_Type()
    {
        // Input and Output
        var schema = new HistoryConversationText
        {
            Role = "user",
            Content = "Test message"
        };

        // Assert
        using (new AssertionScope())
        {
            schema.Type.Should().Be("History");
            schema.Role.Should().Be("user");
            schema.Content.Should().Be("Test message");
        }
    }

    [Test]
    public void HistoryConversationText_ToString_Should_Return_Valid_Json()
    {
        // Input and Output
        var schema = new HistoryConversationText
        {
            Role = "assistant",
            Content = "Based on the current data, it's sunny with a temperature of 72°F (22°C)."
        };

        // Act
        var result = schema.ToString();

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().Contain("History");
            result.Should().Contain("assistant");
            result.Should().Contain("sunny with a temperature");

            // Verify it's valid JSON by parsing it
            var parsed = JsonDocument.Parse(result);
            parsed.RootElement.GetProperty("type").GetString().Should().Be("History");
            parsed.RootElement.GetProperty("role").GetString().Should().Be("assistant");
            parsed.RootElement.GetProperty("content").GetString().Should().Contain("sunny");
        }
    }

    [Test]
    public void HistoryFunctionCalls_Should_Have_Correct_Type()
    {
        // Input and Output
        var schema = new HistoryFunctionCalls
        {
            FunctionCalls = new List<HistoryFunctionCall>
            {
                new HistoryFunctionCall
                {
                    Id = "test-id",
                    Name = "test_function",
                    ClientSide = true,
                    Arguments = "{}",
                    Response = "success"
                }
            }
        };

        // Assert
        using (new AssertionScope())
        {
            schema.Type.Should().Be("History");
            schema.FunctionCalls.Should().HaveCount(1);
            schema.FunctionCalls![0].Name.Should().Be("test_function");
        }
    }

    [Test]
    public void HistoryFunctionCalls_ToString_Should_Return_Valid_Json()
    {
        // Input and Output
        var schema = new HistoryFunctionCalls
        {
            FunctionCalls = new List<HistoryFunctionCall>
            {
                new HistoryFunctionCall
                {
                    Id = "fc_12345678-90ab-cdef-1234-567890abcdef",
                    Name = "check_order_status",
                    ClientSide = true,
                    Arguments = "simple_argument_value",
                    Response = "Order #123456 status: Shipped"
                }
            }
        };

        // Act
        var result = schema.ToString();

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().Contain("History");
            result.Should().Contain("check_order_status");
            result.Should().Contain("fc_12345678-90ab-cdef-1234-567890abcdef");

            // Verify it's valid JSON by parsing it
            var parsed = JsonDocument.Parse(result);
            parsed.RootElement.GetProperty("type").GetString().Should().Be("History");
            parsed.RootElement.GetProperty("function_calls").GetArrayLength().Should().Be(1);
            var functionCall = parsed.RootElement.GetProperty("function_calls")[0];
            functionCall.GetProperty("name").GetString().Should().Be("check_order_status");
            functionCall.GetProperty("client_side").GetBoolean().Should().BeTrue();
        }
    }

    [Test]
    public void HistoryFunctionCall_Should_Serialize_ClientSide_As_Snake_Case()
    {
        // Input and Output
        var functionCall = new HistoryFunctionCall
        {
            Id = "test-id",
            Name = "test_function",
            ClientSide = false,
            Arguments = "{}",
            Response = "success"
        };

        // Act
        var result = functionCall.ToString();

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().Contain("client_side");
            result.Should().Contain("false");

            // Verify it's valid JSON by parsing it
            var parsed = JsonDocument.Parse(result);
            parsed.RootElement.GetProperty("client_side").GetBoolean().Should().BeFalse();
        }
    }

    [Test]
    public void Flags_Should_Have_Default_History_True()
    {
        // Arrange & Act
        var flags = new Flags();

        // Assert
        using (new AssertionScope())
        {
            flags.History.Should().BeTrue();
        }
    }

    [Test]
    public void Flags_Should_Be_Settable()
    {
        // Arrange & Act
        var flags = new Flags
        {
            History = false
        };

        // Assert
        using (new AssertionScope())
        {
            flags.History.Should().BeFalse();
        }
    }

    [Test]
    public void Flags_ToString_Should_Return_Valid_Json()
    {
        // Input and Output
        var flags = new Flags
        {
            History = false
        };

        // Act
        var result = flags.ToString();

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().Contain("history");
            result.Should().Contain("false");

            // Verify it's valid JSON by parsing it
            var parsed = JsonDocument.Parse(result);
            parsed.RootElement.GetProperty("history").GetBoolean().Should().BeFalse();
        }
    }

    [Test]
    public void Context_Should_Allow_Null_Messages()
    {
        // Arrange & Act
        var context = new Context();

        // Assert
        using (new AssertionScope())
        {
            context.Messages.Should().BeNull();
        }
    }

    [Test]
    public void Context_Should_Be_Settable()
    {
        // Arrange & Act
        var context = new Context
        {
            Messages = new List<object>
            {
                new HistoryConversationText { Role = "user", Content = "Hello" }
            }
        };

        // Assert
        using (new AssertionScope())
        {
            context.Messages.Should().HaveCount(1);
        }
    }

    [Test]
    public void Agent_Context_Should_Be_Settable()
    {
        // Arrange & Act
        var agent = new Agent
        {
            Context = new Context
            {
                Messages = new List<object>
                {
                    new HistoryConversationText { Role = "user", Content = "Test" }
                }
            }
        };

        // Assert
        using (new AssertionScope())
        {
            agent.Context.Should().NotBeNull();
            agent.Context!.Messages.Should().HaveCount(1);
        }
    }

    [Test]
    public void SettingsSchema_Flags_Should_Have_Default_Value()
    {
        // Arrange & Act
        var settings = new SettingsSchema();

        // Assert
        using (new AssertionScope())
        {
            settings.Flags.Should().NotBeNull();
            settings.Flags!.History.Should().BeTrue();
        }
    }

    [Test]
    public void SettingsSchema_With_History_Disabled_Should_Serialize_Correctly()
    {
        // Arrange
        var settings = new SettingsSchema
        {
            Flags = new Flags { History = false }
        };

        // Act
        var result = settings.ToString();

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().Contain("flags");
            result.Should().Contain("history");
            result.Should().Contain("false");

            // Verify it's valid JSON by parsing it
            var parsed = JsonDocument.Parse(result);
            parsed.RootElement.GetProperty("flags").GetProperty("history").GetBoolean().Should().BeFalse();
        }
    }

    #endregion
}