// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

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
}