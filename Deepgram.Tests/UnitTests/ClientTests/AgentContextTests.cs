// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Agent.v2.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class AgentContextTests
{
    private static JsonElement Serialize(object value)
    {
        return JsonDocument.Parse(JsonSerializer.Serialize(value, JsonSerializeOptions.DefaultOptions)).RootElement;
    }

    private static SettingsSchema SettingsWithContext(params AgentHistoryMessage[] messages)
    {
        var settings = new SettingsSchema();
        settings.Agent.Context = new AgentContext { Messages = messages.ToList() };
        return settings;
    }

    [Test]
    public void Agent_Should_Not_Emit_Context_When_Unset()
    {
        // Back-compat: every existing caller builds Settings without context, and
        // Agent Settings is strict-checked server side.
        var json = Serialize(new SettingsSchema());

        json.GetProperty("agent").TryGetProperty("context", out _).Should().BeFalse();
    }

    [Test]
    public void FromConversation_Should_Build_A_History_Entry()
    {
        var json = Serialize(AgentHistoryMessage.FromConversation("user", "What is the weather?"));

        using (new AssertionScope())
        {
            json.GetProperty("type").GetString().Should().Be("History");
            json.GetProperty("role").GetString().Should().Be("user");
            json.GetProperty("content").GetString().Should().Be("What is the weather?");
            json.TryGetProperty("function_calls", out _).Should().BeFalse(
                "a conversation entry must not carry an empty function_calls key");
        }
    }

    [Test]
    public void FromFunctionCalls_Should_Build_A_History_Entry()
    {
        var json = Serialize(AgentHistoryMessage.FromFunctionCalls(new[]
        {
            new HistoryFunctionCall
            {
                Id = "call-1",
                Name = "get_weather",
                ClientSide = true,
                Arguments = "{\"city\":\"London\"}",
                Response = "{\"temp_c\":11}",
            },
        }));

        using (new AssertionScope())
        {
            json.GetProperty("type").GetString().Should().Be("History");
            json.TryGetProperty("role", out _).Should().BeFalse(
                "a function-call entry must not carry an empty role key");
            json.TryGetProperty("content", out _).Should().BeFalse();

            var call = json.GetProperty("function_calls").EnumerateArray().Single();
            call.GetProperty("id").GetString().Should().Be("call-1");
            call.GetProperty("name").GetString().Should().Be("get_weather");
            call.GetProperty("client_side").GetBoolean().Should().BeTrue();
            call.GetProperty("arguments").GetString().Should().Be("{\"city\":\"London\"}");
            call.GetProperty("response").GetString().Should().Be("{\"temp_c\":11}");
        }
    }

    [Test]
    public void HistoryFunctionCall_Should_Omit_ThoughtSignature_When_Unset()
    {
        // thought_signature is only required by some Gemini models.
        var json = Serialize(new HistoryFunctionCall { Id = "c", Name = "n", ClientSide = false });

        json.TryGetProperty("thought_signature", out _).Should().BeFalse();
    }

    [Test]
    public void HistoryFunctionCall_Should_Carry_ThoughtSignature_When_Set()
    {
        var json = Serialize(new HistoryFunctionCall { Id = "c", Name = "n", ThoughtSignature = "sig" });

        json.GetProperty("thought_signature").GetString().Should().Be("sig");
    }

    [Test]
    public void Context_Should_Preserve_Message_Order()
    {
        // History is chronological; reordering it changes what the model sees.
        var settings = SettingsWithContext(
            AgentHistoryMessage.FromConversation("user", "first"),
            AgentHistoryMessage.FromConversation("assistant", "second"),
            AgentHistoryMessage.FromConversation("user", "third"));

        var messages = Serialize(settings)
            .GetProperty("agent").GetProperty("context").GetProperty("messages")
            .EnumerateArray().Select(m => m.GetProperty("content").GetString()).ToList();

        messages.Should().ContainInOrder("first", "second", "third");
    }

    [Test]
    public void Context_Should_Mix_Conversation_And_FunctionCall_Entries()
    {
        var settings = SettingsWithContext(
            AgentHistoryMessage.FromConversation("user", "weather in London?"),
            AgentHistoryMessage.FromFunctionCalls(new[]
            {
                new HistoryFunctionCall { Id = "call-1", Name = "get_weather", ClientSide = true },
            }),
            AgentHistoryMessage.FromConversation("assistant", "It is 11 degrees."));

        var messages = Serialize(settings)
            .GetProperty("agent").GetProperty("context").GetProperty("messages")
            .EnumerateArray().ToList();

        using (new AssertionScope())
        {
            messages.Should().HaveCount(3);
            messages[0].GetProperty("role").GetString().Should().Be("user");
            messages[1].GetProperty("function_calls").EnumerateArray().Should().HaveCount(1);
            messages[2].GetProperty("role").GetString().Should().Be("assistant");
            messages.Should().AllSatisfy(m => m.GetProperty("type").GetString().Should().Be("History"));
        }
    }

    [Test]
    public void Context_Should_Serialize_An_Empty_Message_List()
    {
        var settings = SettingsWithContext();

        Serialize(settings)
            .GetProperty("agent").GetProperty("context").GetProperty("messages")
            .EnumerateArray().Should().BeEmpty();
    }

    [Test]
    public void Context_Should_Round_Trip()
    {
        var original = new AgentContext
        {
            Messages = new List<AgentHistoryMessage>
            {
                AgentHistoryMessage.FromConversation("user", "hello"),
                AgentHistoryMessage.FromFunctionCalls(new[]
                {
                    new HistoryFunctionCall { Id = "call-1", Name = "f", ClientSide = true, Arguments = "{}", Response = "{}" },
                }),
            },
        };

        var restored = JsonSerializer.Deserialize<AgentContext>(original.ToString());

        restored.Should().BeEquivalentTo(original);
    }

    [Test]
    public void Context_Should_Coexist_With_The_History_Event_Toggle()
    {
        // Flags.History is a bool that toggles History *event reporting*. It is a
        // different thing from Agent.Context, and setting one must not disturb the other.
        var settings = SettingsWithContext(AgentHistoryMessage.FromConversation("user", "hi"));
        settings.Flags = new AgentFlags { History = false };

        var json = Serialize(settings);

        using (new AssertionScope())
        {
            json.GetProperty("flags").GetProperty("history").GetBoolean().Should().BeFalse();
            json.GetProperty("agent").GetProperty("context")
                .GetProperty("messages").EnumerateArray().Should().HaveCount(1);
        }
    }

    [Test]
    public void Context_Should_Serialize_Alongside_Other_Agent_Settings()
    {
        var settings = SettingsWithContext(AgentHistoryMessage.FromConversation("user", "hi"));
        settings.Agent.Greeting = "Welcome back.";

        var agent = Serialize(settings).GetProperty("agent");

        using (new AssertionScope())
        {
            agent.GetProperty("greeting").GetString().Should().Be("Welcome back.");
            agent.GetProperty("context").GetProperty("messages").EnumerateArray().Should().HaveCount(1);
        }
    }
}
