// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Net.WebSockets;
using System.Text;
using Deepgram.Clients.Agent.v2.WebSocket;
using Deepgram.Models.Agent.v2.WebSocket;
using Deepgram.Models.Authenticate.v1;
using FluentAssertions;
using FluentAssertions.Execution;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class AgentMessageDispatchCoverageTests
{
    private static Client NewClient()
    {
        var options = new DeepgramWsClientOptions("test-api-key") { OnPrem = true };
        return new Client("test-api-key", options);
    }

    private static void FeedTextMessage(Client client, string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        using var stream = new MemoryStream(bytes);
        client.ProcessTextMessage(new WebSocketReceiveResult(bytes.Length, WebSocketMessageType.Text, true), stream);
    }

    private static void FeedTextMessageType(Client client, string type) => FeedTextMessage(client, $$"""{"type":"{{type}}"}""");

    [Test]
    public async Task ProcessTextMessage_Should_Dispatch_Each_Supported_Agent_Event()
    {
        var client = NewClient();

        OpenResponse? opened = null;
        ErrorResponse? error = null;
        AgentAudioDoneResponse? audioDone = null;
        AgentStartedSpeakingResponse? startedSpeaking = null;
        AgentThinkingResponse? thinking = null;
        ConversationTextResponse? conversationText = null;
        FunctionCallRequestResponse? functionCall = null;
        UserStartedSpeakingResponse? userStartedSpeaking = null;
        WelcomeResponse? welcome = null;
        SettingsAppliedResponse? settingsApplied = null;
        InjectionRefusedResponse? injectionRefused = null;
        PromptUpdatedResponse? promptUpdated = null;
        SpeakUpdatedResponse? speakUpdated = null;

        await client.Subscribe(new EventHandler<OpenResponse>((_, response) => opened = response));
        await client.Subscribe(new EventHandler<ErrorResponse>((_, response) => error = response));
        await client.Subscribe(new EventHandler<AgentAudioDoneResponse>((_, response) => audioDone = response));
        await client.Subscribe(new EventHandler<AgentStartedSpeakingResponse>((_, response) => startedSpeaking = response));
        await client.Subscribe(new EventHandler<AgentThinkingResponse>((_, response) => thinking = response));
        await client.Subscribe(new EventHandler<ConversationTextResponse>((_, response) => conversationText = response));
        await client.Subscribe(new EventHandler<FunctionCallRequestResponse>((_, response) => functionCall = response));
        await client.Subscribe(new EventHandler<UserStartedSpeakingResponse>((_, response) => userStartedSpeaking = response));
        await client.Subscribe(new EventHandler<WelcomeResponse>((_, response) => welcome = response));
        await client.Subscribe(new EventHandler<SettingsAppliedResponse>((_, response) => settingsApplied = response));
        await client.Subscribe(new EventHandler<InjectionRefusedResponse>((_, response) => injectionRefused = response));
        await client.Subscribe(new EventHandler<PromptUpdatedResponse>((_, response) => promptUpdated = response));
        await client.Subscribe(new EventHandler<SpeakUpdatedResponse>((_, response) => speakUpdated = response));

        foreach (var json in new[]
                     {
                         """{"type":"Open","request_id":"request-1"}""",
                         """{"type":"Error","code":"CLIENT_MESSAGE_TIMEOUT","description":"timed out"}""",
                         """{"type":"AgentAudioDone"}""",
                         """{"type":"AgentStartedSpeaking","total_latency":1.2,"tts_latency":0.4,"ttt_latency":0.8}""",
                         """{"type":"AgentThinking","content":"checking account"}""",
                         """{"type":"ConversationText","role":"assistant","content":"Hello"}""",
                         """{"type":"FunctionCallRequest","functions":[]}""",
                         """{"type":"UserStartedSpeaking"}""",
                         """{"type":"Welcome","request_id":"request-2"}""",
                         """{"type":"SettingsApplied"}""",
                         """{"type":"InjectionRefused"}""",
                         """{"type":"PromptUpdated"}""",
                         """{"type":"SpeakUpdated"}""",
                     })
        {
            FeedTextMessage(client, json);
        }

        using (new AssertionScope())
        {
            opened.Should().NotBeNull();
            error!.Code.Should().Be("CLIENT_MESSAGE_TIMEOUT");
            error.Description.Should().Be("timed out");
            audioDone.Should().NotBeNull();
            startedSpeaking!.TotalLatency.Should().Be(1.2m);
            startedSpeaking.TtsLatency.Should().Be(0.4m);
            thinking!.Content.Should().Be("checking account");
            conversationText!.Role.Should().Be("assistant");
            conversationText.Content.Should().Be("Hello");
            functionCall.Should().NotBeNull();
            userStartedSpeaking.Should().NotBeNull();
            welcome!.RequestId.Should().Be("request-2");
            settingsApplied.Should().NotBeNull();
            injectionRefused.Should().NotBeNull();
            promptUpdated.Should().NotBeNull();
            speakUpdated.Should().NotBeNull();
        }
    }

    [Test]
    public async Task ProcessBinaryMessage_Should_Dispatch_Agent_Audio()
    {
        var client = NewClient();
        AudioResponse? audio = null;
        await client.Subscribe(new EventHandler<AudioResponse>((_, response) => audio = response));

        var bytes = new byte[] { 1, 2, 3, 4 };
        using var stream = new MemoryStream(bytes);
        client.ProcessBinaryMessage(new WebSocketReceiveResult(bytes.Length, WebSocketMessageType.Binary, true), stream);

        audio.Should().NotBeNull();
        audio!.Stream.Should().NotBeNull();
        audio.Stream!.ToArray().Should().Equal(bytes);
    }

    [TestCase("AgentAudioDone")]
    [TestCase("AgentStartedSpeaking")]
    [TestCase("AgentThinking")]
    [TestCase("ConversationText")]
    [TestCase("FunctionCallRequest")]
    [TestCase("UserStartedSpeaking")]
    [TestCase("Welcome")]
    [TestCase("SettingsApplied")]
    [TestCase("InjectionRefused")]
    [TestCase("PromptUpdated")]
    [TestCase("SpeakUpdated")]
    public void ProcessTextMessage_Without_A_Typed_Subscriber_Should_Not_Throw(string type)
    {
        var client = NewClient();

        Action act = () => FeedTextMessageType(client, type);

        act.Should().NotThrow();
    }

    [Test]
    public void ProcessBinaryMessage_Without_An_Audio_Subscriber_Should_Not_Throw()
    {
        var client = NewClient();
        var bytes = new byte[] { 1, 2, 3, 4 };
        using var stream = new MemoryStream(bytes);

        Action act = () => client.ProcessBinaryMessage(
            new WebSocketReceiveResult(bytes.Length, WebSocketMessageType.Binary, true), stream);

        act.Should().NotThrow();
    }

    [Test]
    public void GetUri_Should_Use_The_Agent_Host_And_Remove_The_Api_Version()
    {
        var options = new DeepgramWsClientOptions("test-api-key") { OnPrem = true };

        var uri = Client.GetUri(options);

        uri.Should().Be(new Uri("wss://agent.deepgram.com/v1/agent/converse"));
    }
}
