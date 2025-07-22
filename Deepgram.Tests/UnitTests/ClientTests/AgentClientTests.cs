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

public class AgentClientTests
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

    [Test]
    public async Task SendInjectUserMessage_With_String_Should_Send_Message()
    {
        // Input and Output
        var content = "Hello! Can you hear me?";
        var agentClient = Substitute.For<Client>(_apiKey, _options);

        // Mock the SendMessageImmediately method
        agentClient.When(x => x.SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<CancellationTokenSource>()))
                   .DoNotCallBase();

        // Act
        await agentClient.SendInjectUserMessage(content);

        // Assert
        await agentClient.Received(1).SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<CancellationTokenSource>());
    }

    [Test]
    public async Task SendInjectUserMessage_With_Schema_Should_Send_Message()
    {
        // Input and Output
        var schema = new InjectUserMessageSchema
        {
            Content = "Hello! Can you hear me?"
        };
        var agentClient = Substitute.For<Client>(_apiKey, _options);

        // Mock the SendMessageImmediately method
        agentClient.When(x => x.SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<CancellationTokenSource>()))
                   .DoNotCallBase();

        // Act
        await agentClient.SendInjectUserMessage(schema);

        // Assert
        await agentClient.Received(1).SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<CancellationTokenSource>());
    }

    [Test]
    public async Task SendInjectUserMessage_With_Null_String_Should_Throw_ArgumentException()
    {
        // Input and Output
        string? content = null;
        var agentClient = new Client(_apiKey, _options);

        // Act & Assert
        var exception = await agentClient.Invoking(y => y.SendInjectUserMessage(content!))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Content cannot be null or empty*");
        exception.And.ParamName.Should().Be("content");
    }

    [Test]
    public async Task SendInjectUserMessage_With_Empty_String_Should_Throw_ArgumentException()
    {
        // Input and Output
        var content = "";
        var agentClient = new Client(_apiKey, _options);

        // Act & Assert
        var exception = await agentClient.Invoking(y => y.SendInjectUserMessage(content))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Content cannot be null or empty*");
        exception.And.ParamName.Should().Be("content");
    }

    [Test]
    public async Task SendInjectUserMessage_With_Whitespace_String_Should_Throw_ArgumentException()
    {
        // Input and Output
        var content = "   ";
        var agentClient = new Client(_apiKey, _options);

        // Act & Assert
        var exception = await agentClient.Invoking(y => y.SendInjectUserMessage(content))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Content cannot be null or empty*");
        exception.And.ParamName.Should().Be("content");
    }

    [Test]
    public async Task SendInjectUserMessage_With_Null_Schema_Should_Throw_ArgumentNullException()
    {
        // Input and Output
        InjectUserMessageSchema? schema = null;
        var agentClient = new Client(_apiKey, _options);

        // Act & Assert
        var exception = await agentClient.Invoking(y => y.SendInjectUserMessage(schema!))
            .Should().ThrowAsync<ArgumentNullException>();
        exception.And.ParamName.Should().Be("injectUserMessageSchema");
    }

    [Test]
    public async Task SendInjectUserMessage_With_Schema_Null_Content_Should_Throw_ArgumentException()
    {
        // Input and Output
        var schema = new InjectUserMessageSchema
        {
            Content = null
        };
        var agentClient = new Client(_apiKey, _options);

        // Act & Assert
        var exception = await agentClient.Invoking(y => y.SendInjectUserMessage(schema))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Content cannot be null or empty*");
        exception.And.ParamName.Should().Be("Content");
    }

    [Test]
    public async Task SendInjectUserMessage_With_Schema_Empty_Content_Should_Throw_ArgumentException()
    {
        // Input and Output
        var schema = new InjectUserMessageSchema
        {
            Content = ""
        };
        var agentClient = new Client(_apiKey, _options);

        // Act & Assert
        var exception = await agentClient.Invoking(y => y.SendInjectUserMessage(schema))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Content cannot be null or empty*");
        exception.And.ParamName.Should().Be("Content");
    }

    [Test]
    public void InjectUserMessageSchema_Should_Have_Correct_Type()
    {
        // Input and Output
        var schema = new InjectUserMessageSchema
        {
            Content = "Test message"
        };

        // Assert
        using (new AssertionScope())
        {
            schema.Type.Should().Be("InjectUserMessage");
            schema.Content.Should().Be("Test message");
        }
    }

    [Test]
    public void InjectUserMessageSchema_ToString_Should_Return_Valid_Json()
    {
        // Input and Output
        var schema = new InjectUserMessageSchema
        {
            Content = "Hello! Can you hear me?"
        };

        // Act
        var result = schema.ToString();

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().Contain("InjectUserMessage");
            result.Should().Contain("Hello! Can you hear me?");

            // Verify it's valid JSON by parsing it
            var parsed = JsonDocument.Parse(result);
            parsed.RootElement.GetProperty("type").GetString().Should().Be("InjectUserMessage");
            parsed.RootElement.GetProperty("content").GetString().Should().Be("Hello! Can you hear me?");
        }
    }

    #region MipOptOut Tests

    [Test]
    public void SettingsSchema_MipOptOut_Should_Have_Default_Value_False()
    {
        // Arrange & Act
        var settings = new SettingsSchema();

        // Assert
        using (new AssertionScope())
        {
            settings.MipOptOut.Should().BeFalse();
        }
    }

    [Test]
    public void SettingsSchema_MipOptOut_Should_Be_Settable()
    {
        // Arrange & Act
        var settings = new SettingsSchema
        {
            MipOptOut = true
        };

        // Assert
        using (new AssertionScope())
        {
            settings.MipOptOut.Should().BeTrue();
        }
    }

    [Test]
    public void SettingsSchema_MipOptOut_Should_Serialize_To_Snake_Case()
    {
        // Arrange
        var settings = new SettingsSchema
        {
            MipOptOut = true
        };

        // Act
        var result = settings.ToString();

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().Contain("mip_opt_out");
            result.Should().Contain("true");

            // Verify it's valid JSON by parsing it
            var parsed = JsonDocument.Parse(result);
            parsed.RootElement.GetProperty("mip_opt_out").GetBoolean().Should().BeTrue();
        }
    }

    [Test]
    public void SettingsSchema_MipOptOut_False_Should_Serialize_Correctly()
    {
        // Arrange
        var settings = new SettingsSchema
        {
            MipOptOut = false
        };

        // Act
        var result = settings.ToString();

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().Contain("mip_opt_out");
            result.Should().Contain("false");

            // Verify it's valid JSON by parsing it
            var parsed = JsonDocument.Parse(result);
            parsed.RootElement.GetProperty("mip_opt_out").GetBoolean().Should().BeFalse();
        }
    }

    [Test]
    public void SettingsSchema_MipOptOut_Null_Should_Not_Serialize()
    {
        // Arrange
        var settings = new SettingsSchema
        {
            MipOptOut = null
        };

        // Act
        var result = settings.ToString();

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().NotContain("mip_opt_out");

            // Verify it's valid JSON by parsing it
            var parsed = JsonDocument.Parse(result);
            parsed.RootElement.TryGetProperty("mip_opt_out", out _).Should().BeFalse();
        }
    }

    [Test]
    public void SettingsSchema_With_MipOptOut_Should_Serialize_With_Other_Properties()
    {
        // Arrange
        var settings = new SettingsSchema
        {
            Experimental = true,
            MipOptOut = true,
            Agent = new Agent
            {
                Language = "en",
                Greeting = "Hello, I'm your agent"
            }
        };

        // Act
        var result = settings.ToString();

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().Contain("experimental");
            result.Should().Contain("mip_opt_out");
            result.Should().Contain("agent");

            // Verify it's valid JSON by parsing it
            var parsed = JsonDocument.Parse(result);
            parsed.RootElement.GetProperty("experimental").GetBoolean().Should().BeTrue();
            parsed.RootElement.GetProperty("mip_opt_out").GetBoolean().Should().BeTrue();
            parsed.RootElement.GetProperty("agent").GetProperty("language").GetString().Should().Be("en");
            parsed.RootElement.GetProperty("agent").GetProperty("greeting").GetString().Should().Be("Hello, I'm your agent");
        }
    }

    [Test]
    public void SettingsSchema_MipOptOut_Schema_Should_Match_API_Specification()
    {
        // Arrange - Test both default (false) and explicit true values
        var settingsDefault = new SettingsSchema();
        var settingsOptOut = new SettingsSchema { MipOptOut = true };

        // Act
        var defaultResult = settingsDefault.ToString();
        var optOutResult = settingsOptOut.ToString();

        // Assert
        using (new AssertionScope())
        {
            // Default should be false
            var defaultParsed = JsonDocument.Parse(defaultResult);
            defaultParsed.RootElement.GetProperty("mip_opt_out").GetBoolean().Should().BeFalse();

            // Explicit true should be true
            var optOutParsed = JsonDocument.Parse(optOutResult);
            optOutParsed.RootElement.GetProperty("mip_opt_out").GetBoolean().Should().BeTrue();
        }
    }

    [Test]
    public void Agent_Should_Not_Have_MipOptOut_Property()
    {
        // Arrange
        var agent = new Agent
        {
            Language = "en",
            Greeting = "Hello, I'm your agent"
        };

        // Act
        var result = agent.ToString();

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().NotContain("mip_opt_out");

            // Verify it's valid JSON by parsing it
            var parsed = JsonDocument.Parse(result);
            parsed.RootElement.TryGetProperty("mip_opt_out", out _).Should().BeFalse();
        }
    }

    #endregion

    #region Tags Tests

    [Test]
    public void Agent_Tags_Should_Have_Default_Value_Null()
    {
        // Arrange & Act
        var agent = new Agent();

        // Assert
        using (new AssertionScope())
        {
            agent.Tags.Should().BeNull();
        }
    }

    [Test]
    public void Agent_Tags_Should_Be_Settable()
    {
        // Arrange & Act
        var agent = new Agent
        {
            Tags = new List<string> { "test", "demo", "agent" }
        };

        // Assert
        using (new AssertionScope())
        {
            agent.Tags.Should().NotBeNull();
            agent.Tags.Should().HaveCount(3);
            agent.Tags.Should().Contain("test");
            agent.Tags.Should().Contain("demo");
            agent.Tags.Should().Contain("agent");
        }
    }

    [Test]
    public void Agent_Tags_Should_Serialize_To_Json_Array()
    {
        // Arrange
        var agent = new Agent
        {
            Tags = new List<string> { "production", "voice-bot", "customer-service" }
        };

        // Act
        var result = agent.ToString();

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().Contain("tags");
            result.Should().Contain("[");
            result.Should().Contain("]");
            result.Should().Contain("production");
            result.Should().Contain("voice-bot");
            result.Should().Contain("customer-service");

            // Verify it's valid JSON by parsing it
            var parsed = JsonDocument.Parse(result);
            var tagsArray = parsed.RootElement.GetProperty("tags");
            tagsArray.ValueKind.Should().Be(JsonValueKind.Array);
            tagsArray.GetArrayLength().Should().Be(3);

            var tagsList = new List<string>();
            foreach (var tag in tagsArray.EnumerateArray())
            {
                tagsList.Add(tag.GetString()!);
            }
            tagsList.Should().Contain("production");
            tagsList.Should().Contain("voice-bot");
            tagsList.Should().Contain("customer-service");
        }
    }

    [Test]
    public void Agent_Tags_Empty_List_Should_Serialize_As_Empty_Array()
    {
        // Arrange
        var agent = new Agent
        {
            Tags = new List<string>()
        };

        // Act
        var result = agent.ToString();

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().Contain("tags");
            result.Should().Contain("[]");

            // Verify it's valid JSON by parsing it
            var parsed = JsonDocument.Parse(result);
            var tagsArray = parsed.RootElement.GetProperty("tags");
            tagsArray.ValueKind.Should().Be(JsonValueKind.Array);
            tagsArray.GetArrayLength().Should().Be(0);
        }
    }

    [Test]
    public void Agent_Tags_Null_Should_Not_Serialize()
    {
        // Arrange
        var agent = new Agent
        {
            Tags = null
        };

        // Act
        var result = agent.ToString();

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().NotContain("tags");

            // Verify it's valid JSON by parsing it
            var parsed = JsonDocument.Parse(result);
            parsed.RootElement.TryGetProperty("tags", out _).Should().BeFalse();
        }
    }

    [Test]
    public void Agent_With_Tags_Should_Serialize_With_Other_Properties()
    {
        // Arrange
        var agent = new Agent
        {
            Language = "en",
            Greeting = "Hello, I'm your agent",
            Tags = new List<string> { "test-tag", "integration" },
            MipOptOut = true
        };

        // Act
        var result = agent.ToString();

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().Contain("language");
            result.Should().Contain("greeting");
            result.Should().Contain("tags");
            result.Should().Contain("mip_opt_out");

            // Verify it's valid JSON by parsing it
            var parsed = JsonDocument.Parse(result);
            parsed.RootElement.GetProperty("language").GetString().Should().Be("en");
            parsed.RootElement.GetProperty("greeting").GetString().Should().Be("Hello, I'm your agent");
            parsed.RootElement.GetProperty("mip_opt_out").GetBoolean().Should().BeTrue();

            var tagsArray = parsed.RootElement.GetProperty("tags");
            tagsArray.ValueKind.Should().Be(JsonValueKind.Array);
            tagsArray.GetArrayLength().Should().Be(2);
        }
    }

    [Test]
    public void Agent_Tags_Should_Support_Special_Characters()
    {
        // Arrange
        var agent = new Agent
        {
            Tags = new List<string> { "test-with-dashes", "test_with_underscores", "test with spaces", "test.with.dots" }
        };

        // Act
        var result = agent.ToString();

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();

            // Verify it's valid JSON by parsing it
            var parsed = JsonDocument.Parse(result);
            var tagsArray = parsed.RootElement.GetProperty("tags");
            tagsArray.ValueKind.Should().Be(JsonValueKind.Array);
            tagsArray.GetArrayLength().Should().Be(4);

            var tagsList = new List<string>();
            foreach (var tag in tagsArray.EnumerateArray())
            {
                tagsList.Add(tag.GetString()!);
            }
            tagsList.Should().Contain("test-with-dashes");
            tagsList.Should().Contain("test_with_underscores");
            tagsList.Should().Contain("test with spaces");
            tagsList.Should().Contain("test.with.dots");
        }
    }

    [Test]
    public void Agent_Tags_Schema_Should_Match_API_Specification()
    {
        // Arrange - Test various scenarios as per API specification
        var agentWithTags = new Agent { Tags = new List<string> { "search-filter", "analytics", "production" } };
        var agentWithoutTags = new Agent { Tags = null };
        var agentWithEmptyTags = new Agent { Tags = new List<string>() };

        // Act
        var withTagsResult = agentWithTags.ToString();
        var withoutTagsResult = agentWithoutTags.ToString();
        var emptyTagsResult = agentWithEmptyTags.ToString();

        // Assert
        using (new AssertionScope())
        {
            // With tags should serialize array
            var withTagsParsed = JsonDocument.Parse(withTagsResult);
            var tagsArray = withTagsParsed.RootElement.GetProperty("tags");
            tagsArray.ValueKind.Should().Be(JsonValueKind.Array);
            tagsArray.GetArrayLength().Should().Be(3);

            // Without tags should not include tags property
            var withoutTagsParsed = JsonDocument.Parse(withoutTagsResult);
            withoutTagsParsed.RootElement.TryGetProperty("tags", out _).Should().BeFalse();

            // Empty tags should serialize as empty array
            var emptyTagsParsed = JsonDocument.Parse(emptyTagsResult);
            var emptyTagsArray = emptyTagsParsed.RootElement.GetProperty("tags");
            emptyTagsArray.ValueKind.Should().Be(JsonValueKind.Array);
            emptyTagsArray.GetArrayLength().Should().Be(0);
        }
    }

    #endregion
}