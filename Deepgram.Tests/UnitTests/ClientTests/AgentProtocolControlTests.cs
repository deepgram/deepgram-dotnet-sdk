// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Text;
using Deepgram.Clients.Interfaces.v2;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Agent.v2.WebSocket;
using AgentConstants = Deepgram.Clients.Interfaces.v2.Constants;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class AgentProtocolControlTests
{
    private static JsonElement JsonValue(string value)
    {
        using var document = JsonDocument.Parse(value);
        return document.RootElement.Clone();
    }

    [Test]
    public async Task AgentProtocolExtensions_Should_Serialize_Current_Control_Messages()
    {
        var client = Substitute.For<IAgentWebSocketClient>();
        var payloads = new List<JsonDocument>();
        client.SendMessageImmediately(Arg.Do<byte[]>(data => payloads.Add(JsonDocument.Parse(Encoding.UTF8.GetString(data)))),
            Arg.Any<int>(), Arg.Any<CancellationTokenSource>()).Returns(Task.CompletedTask);

        await client.SendUpdateListen(new AgentUpdateListenSchema { Listen = new Listen { Provider = new Provider { Type = "deepgram" } } });
        dynamic thinkProvider = new Provider { Type = "open_ai" };
        thinkProvider.Model = "gpt-4o-mini";
        dynamic speakProvider = new Provider { Type = "deepgram" };
        speakProvider.Model = "aura-2-thalia-en";
        dynamic awsPollyProvider = new Provider { Type = "aws_polly" };
        dynamic cartesiaProvider = new Provider { Type = "cartesia" };
        cartesiaProvider.ModelId = "sonic-english";
        await client.SendUpdateThink(new AgentUpdateThinkSchema { Think = new Think { Provider = thinkProvider, Prompt = "Be concise" } });
        await client.SendUpdateSpeak(new AgentUpdateSpeakSchema
        {
            Speak = new Speak
            {
                SpeakProviders = new List<SpeakProviderConfig>
                {
                    new() { Provider = speakProvider },
                    new() { Provider = awsPollyProvider },
                    new() { Provider = cartesiaProvider },
                },
            },
        });
        await client.SendUpdatePrompt(new AgentUpdatePromptSchema { Prompt = "Use short answers" });
        await client.SendInjectAgentMessage(new AgentInjectAgentMessageSchema { Message = "One moment", Behavior = "queue" });
        await client.SendFunctionCallResponse(new AgentFunctionCallResponseSchema { Id = "call-1", Name = "get_weather", Content = "{}" });
        await client.SendCustomToThinkProvider(new AgentCustomToThinkProviderSchema { Content = JsonValue("{\"action\":\"continue\"}") });

        var protocolClient = Substitute.For<IAgentProtocolClient>();
        protocolClient.SendForceEndTurn().Returns(Task.CompletedTask);
        await ((IAgentWebSocketClient)protocolClient).SendForceEndTurn();

        using (new AssertionScope())
        {
            payloads.Select(document => document.RootElement.GetProperty("type").GetString()).Should().Equal(
                "UpdateListen", "UpdateThink", "UpdateSpeak", "UpdatePrompt", "InjectAgentMessage", "FunctionCallResponse", "__customToThinkProvider");
            payloads[4].RootElement.GetProperty("message").GetString().Should().Be("One moment");
            payloads[4].RootElement.GetProperty("behavior").GetString().Should().Be("queue");
            payloads[2].RootElement.GetProperty("speak").ValueKind.Should().Be(JsonValueKind.Array);
            payloads[2].RootElement.GetProperty("speak")[0].GetProperty("provider").GetProperty("model").GetString().Should().Be("aura-2-thalia-en");
            payloads[2].RootElement.GetProperty("speak")[1].GetProperty("provider").GetProperty("type").GetString().Should().Be("aws_polly");
            payloads[2].RootElement.GetProperty("speak")[2].GetProperty("provider").GetProperty("model_id").GetString().Should().Be("sonic-english");
            payloads[5].RootElement.GetProperty("id").GetString().Should().Be("call-1");
            payloads[5].RootElement.GetProperty("content").GetString().Should().Be("{}");
            payloads[6].RootElement.GetProperty("content").GetProperty("action").GetString().Should().Be("continue");
            await protocolClient.Received(1).SendForceEndTurn();
        }

        foreach (var payload in payloads)
        {
            payload.Dispose();
        }
    }

    [Test]
    public async Task AgentProtocolExtensions_Should_Reject_Incomplete_Control_Messages()
    {
        var client = Substitute.For<IAgentWebSocketClient>();

        await client.Invoking(c => c.SendUpdateListen(new AgentUpdateListenSchema()))
            .Should().ThrowAsync<ArgumentException>();
        await client.Invoking(c => c.SendUpdateThink(new AgentUpdateThinkSchema()))
            .Should().ThrowAsync<ArgumentException>();
        await client.Invoking(c => c.SendUpdateSpeak(new AgentUpdateSpeakSchema()))
            .Should().ThrowAsync<ArgumentException>();
        await client.Invoking(c => c.SendUpdatePrompt(new AgentUpdatePromptSchema()))
            .Should().ThrowAsync<ArgumentException>();
        await client.Invoking(c => c.SendInjectAgentMessage(new AgentInjectAgentMessageSchema()))
            .Should().ThrowAsync<ArgumentException>();
        await client.Invoking(c => c.SendFunctionCallResponse(new AgentFunctionCallResponseSchema { Name = "fn" }))
            .Should().ThrowAsync<ArgumentException>();
        await client.Invoking(c => c.SendFunctionCallResponse(new AgentFunctionCallResponseSchema { Name = "fn", Content = "{}" }))
            .Should().ThrowAsync<ArgumentException>();
        await client.Invoking(c => c.SendCustomToThinkProvider(new AgentCustomToThinkProviderSchema()))
            .Should().ThrowAsync<ArgumentException>();
        await client.Invoking(c => c.SendUpdateThink(new AgentUpdateThinkSchema { Think = new Think { Prompt = "missing provider" } }))
            .Should().ThrowAsync<ArgumentException>();
        await client.Invoking(c => c.SendUpdateSpeak(new AgentUpdateSpeakSchema { Speak = new Speak() }))
            .Should().ThrowAsync<ArgumentException>();
        dynamic fluxProvider = new Provider { Type = "deepgram" };
        fluxProvider.Version = "v2";
        await client.Invoking(c => c.SendUpdateListen(new AgentUpdateListenSchema { Listen = new Listen { Provider = fluxProvider } }))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public void Function_Should_Serialize_DeferUntilEot_When_Set()
    {
        var function = new Function { Name = "charge_card", DeferUntilEot = true };

        using var document = JsonDocument.Parse(function.ToString());
        document.RootElement.GetProperty("defer_until_eot").GetBoolean().Should().BeTrue();
    }

    [Test]
    public async Task AgentFunctionCallResponse_Should_Allow_Empty_Content()
    {
        var client = Substitute.For<IAgentWebSocketClient>();
        client.SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<CancellationTokenSource>()).Returns(Task.CompletedTask);

        await client.SendFunctionCallResponse(new AgentFunctionCallResponseSchema { Id = "call-1", Name = "lookup", Content = "" });

        await client.Received(1).SendMessageImmediately(
            Arg.Is<byte[]>(payload => JsonDocument.Parse(Encoding.UTF8.GetString(payload), default).RootElement.GetProperty("content").GetString() == ""),
            AgentConstants.UseArrayLengthForSend, null);
    }

    [TestCase("{\"request\":\"continue\"}")]
    [TestCase("[\"next\",2]")]
    [TestCase("\"plain text\"")]
    [TestCase("null")]
    public async Task CustomToThinkProvider_Should_Serialize_Any_Json_Value(string content)
    {
        var client = Substitute.For<IAgentWebSocketClient>();
        var payloads = new List<JsonDocument>();
        client.SendMessageImmediately(Arg.Do<byte[]>(data => payloads.Add(JsonDocument.Parse(Encoding.UTF8.GetString(data)))),
            Arg.Any<int>(), Arg.Any<CancellationTokenSource>()).Returns(Task.CompletedTask);

        await client.SendCustomToThinkProvider(new AgentCustomToThinkProviderSchema { Content = JsonValue(content) });

        payloads.Should().ContainSingle();
        payloads[0].RootElement.GetProperty("type").GetString().Should().Be("__customToThinkProvider");
        JsonSerializer.Serialize(payloads[0].RootElement.GetProperty("content"))
            .Should().Be(JsonSerializer.Serialize(JsonValue(content)));
        payloads[0].Dispose();
    }

    [Test]
    public async Task AgentProtocolClient_Should_Await_ForceEndTurn_Send()
    {
        var options = new DeepgramWsClientOptions("test-api-key") { OnPrem = true };
        var client = Substitute.For<Deepgram.Clients.Agent.v2.WebSocket.Client>("test-api-key", options);
        client.SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<CancellationTokenSource>()).Returns(Task.CompletedTask);

        await client.SendForceEndTurn();

        await client.Received(1).SendMessageImmediately(
            Arg.Is<byte[]>(payload => JsonDocument.Parse(Encoding.UTF8.GetString(payload), default).RootElement.GetProperty("type").GetString() == "ForceEndTurn"),
            AgentConstants.UseArrayLengthForSend, null);
    }

    [Test]
    public async Task LegacyAgentClient_Should_Reject_ForceEndTurn_Without_Protocol_Delivery()
    {
        var client = Substitute.For<IAgentWebSocketClient>();

        await client.Invoking(c => c.SendForceEndTurn())
            .Should().ThrowAsync<NotSupportedException>();
    }

    [Test]
    public void SettingsSchema_Should_Serialize_History_Flag()
    {
        var settings = new SettingsSchema { Flags = new AgentFlags { History = false } };

        using var document = JsonDocument.Parse(settings.ToString());
        document.RootElement.GetProperty("flags").GetProperty("history").GetBoolean().Should().BeFalse();
    }

    [Test]
    public void AgentHistoryResponse_Should_Deserialize_ThoughtSignature()
    {
        var response = JsonSerializer.Deserialize<AgentHistoryResponse>(
            """{"type":"History","function_calls":[{"id":"call-1","name":"lookup","client_side":true,"arguments":"{}","response":"{}","thought_signature":"sig"}]}""");

        response!.FunctionCalls.Should().ContainSingle();
        response.FunctionCalls![0].ThoughtSignature.Should().Be("sig");
    }

    [Test]
    public void FunctionCallRequestResponse_Should_Deserialize_ThoughtSignature()
    {
        var response = JsonSerializer.Deserialize<FunctionCallRequestResponse>(
            """{"type":"FunctionCallRequest","functions":[{"id":"call-1","name":"lookup","arguments":"{}","client_side":true,"thought_signature":"sig"}]}""");

        response!.Functions.Should().ContainSingle();
        response.Functions![0].ThoughtSignature.Should().Be("sig");
    }
}
