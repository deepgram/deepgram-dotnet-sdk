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
        ListenUpdatedResponse? listenUpdated = null;
        ThinkUpdatedResponse? thinkUpdated = null;
        FunctionCallCancelledResponse? functionCallCancelled = null;
        FunctionCallResponse? functionCallResponse = null;
        CustomFromThinkProviderResponse? customFromThinkProvider = null;
        LatencyReportResponse? latencyReport = null;
        AgentWarningResponse? warning = null;
        AgentHistoryResponse? history = null;

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
        await client.Subscribe(new EventHandler<ListenUpdatedResponse>((_, response) => listenUpdated = response));
        await client.Subscribe(new EventHandler<ThinkUpdatedResponse>((_, response) => thinkUpdated = response));
        await client.Subscribe(new EventHandler<FunctionCallCancelledResponse>((_, response) => functionCallCancelled = response));
        await client.Subscribe(new EventHandler<FunctionCallResponse>((_, response) => functionCallResponse = response));
        await client.Subscribe(new EventHandler<CustomFromThinkProviderResponse>((_, response) => customFromThinkProvider = response));
        await client.Subscribe(new EventHandler<LatencyReportResponse>((_, response) => latencyReport = response));
        await client.Subscribe(new EventHandler<AgentWarningResponse>((_, response) => warning = response));
        await client.Subscribe(new EventHandler<AgentHistoryResponse>((_, response) => history = response));

        foreach (var json in new[]
                     {
                         """{"type":"Open","request_id":"request-1"}""",
                         """{"type":"Error","code":"CLIENT_MESSAGE_TIMEOUT","description":"timed out"}""",
                         """{"type":"AgentAudioDone"}""",
                          """{"type":"AgentStartedSpeaking","total_latency":"1.2","tts_latency":"0.4","ttt_latency":"0.8"}""",
                         """{"type":"AgentThinking","content":"checking account"}""",
                          """{"type":"ConversationText","role":"assistant","content":"Hello","languages_hinted":["en","es"],"languages":["en"]}""",
                         """{"type":"FunctionCallRequest","functions":[]}""",
                         """{"type":"UserStartedSpeaking"}""",
                         """{"type":"Welcome","request_id":"request-2"}""",
                         """{"type":"SettingsApplied"}""",
                          """{"type":"InjectionRefused","message":"Agent is speaking"}""",
                          """{"type":"PromptUpdated"}""",
                          """{"type":"SpeakUpdated"}""",
                          """{"type":"ListenUpdated"}""",
                          """{"type":"ThinkUpdated"}""",
                          """{"type":"FunctionCallCancelled","functions":[{"id":"call-1","name":"charge_card"}]}""",
                          """{"type":"FunctionCallResponse","id":"call-2","name":"lookup","content":"{}"}""",
                          """{"type":"__customFromThinkProvider","content":{"next":"continue","attempt":2}}""",
                          """{"type":"LatencyReport","stt_latency":"0.2","total_latency":"1.1"}""",
                          """{"type":"Warning","code":"FORCE_END_TURN_UNSUPPORTED","description":"requires Flux"}""",
                          """{"type":"History","role":"assistant","content":"Welcome back"}""",
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
            conversationText.LanguagesHinted.Should().Equal("en", "es");
            conversationText.Languages.Should().Equal("en");
            functionCall.Should().NotBeNull();
            userStartedSpeaking.Should().NotBeNull();
            welcome!.RequestId.Should().Be("request-2");
            settingsApplied.Should().NotBeNull();
            injectionRefused!.Message.Should().Be("Agent is speaking");
            promptUpdated.Should().NotBeNull();
            speakUpdated.Should().NotBeNull();
            listenUpdated.Should().NotBeNull();
            thinkUpdated.Should().NotBeNull();
            functionCallCancelled!.Functions.Should().ContainSingle();
            functionCallCancelled.Functions![0].Id.Should().Be("call-1");
            functionCallResponse!.Id.Should().Be("call-2");
            functionCallResponse.Name.Should().Be("lookup");
            customFromThinkProvider!.Content.GetProperty("next").GetString().Should().Be("continue");
            latencyReport!.SttLatency.Should().Be(0.2m);
            latencyReport.TotalLatency.Should().Be(1.1m);
            warning!.Code.Should().Be("FORCE_END_TURN_UNSUPPORTED");
            history!.Role.Should().Be("assistant");
            history.Content.Should().Be("Welcome back");
        }
    }

    [TestCase("""{"type":"AgentStartedSpeaking","total_latency":1.2,"tts_latency":0.4,"ttt_latency":0.8}""")]
    [TestCase("""{"type":"AgentStartedSpeaking","total_latency":"1.2","tts_latency":"0.4","ttt_latency":"0.8"}""")]
    public async Task ProcessTextMessage_Should_Read_AgentStartedSpeaking_Latencies_As_Numbers_Or_Strings(string json)
    {
        var client = NewClient();
        AgentStartedSpeakingResponse? startedSpeaking = null;
        await client.Subscribe(new EventHandler<AgentStartedSpeakingResponse>((_, response) => startedSpeaking = response));

        FeedTextMessage(client, json);

        using (new AssertionScope())
        {
            startedSpeaking!.TotalLatency.Should().Be(1.2m);
            startedSpeaking.TtsLatency.Should().Be(0.4m);
            startedSpeaking.TttLatency.Should().Be(0.8m);
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
    [TestCase("ListenUpdated")]
    [TestCase("ThinkUpdated")]
    [TestCase("FunctionCallCancelled")]
    [TestCase("FunctionCallResponse")]
    [TestCase("__customFromThinkProvider")]
    [TestCase("LatencyReport")]
    [TestCase("Warning")]
    [TestCase("History")]
    public void ProcessTextMessage_Without_A_Typed_Subscriber_Should_Not_Throw(string type)
    {
        var client = NewClient();

        Action act = () => FeedTextMessageType(client, type);

        act.Should().NotThrow();
    }

    [TestCase("{\"result\":\"continue\"}", JsonValueKind.Object)]
    [TestCase("[\"next\",2]", JsonValueKind.Array)]
    [TestCase("\"plain text\"", JsonValueKind.String)]
    [TestCase("42", JsonValueKind.Number)]
    [TestCase("true", JsonValueKind.True)]
    [TestCase("null", JsonValueKind.Null)]
    public async Task ProcessTextMessage_Should_Dispatch_CustomThinkProvider_Any_Json_Content(string content, JsonValueKind kind)
    {
        var client = NewClient();
        CustomFromThinkProviderResponse? customFromThinkProvider = null;
        await client.Subscribe(new EventHandler<CustomFromThinkProviderResponse>((_, response) => customFromThinkProvider = response));

        FeedTextMessage(client, $$"""{"type":"__customFromThinkProvider","content":{{content}}}""");

        using var expected = JsonDocument.Parse(content);
        customFromThinkProvider.Should().NotBeNull();
        customFromThinkProvider!.Content.ValueKind.Should().Be(kind);
        JsonSerializer.Serialize(customFromThinkProvider.Content).Should().Be(JsonSerializer.Serialize(expected.RootElement));
    }

    [Test]
    public async Task ProcessTextMessage_With_Missing_CustomThinkProvider_Content_Should_Remain_Unhandled()
    {
        var client = NewClient();
        CustomFromThinkProviderResponse? customFromThinkProvider = null;
        UnhandledResponse? unhandled = null;
        await client.Subscribe(new EventHandler<CustomFromThinkProviderResponse>((_, response) => customFromThinkProvider = response));
        await client.Subscribe(new EventHandler<UnhandledResponse>((_, response) => unhandled = response));

        FeedTextMessage(client, """{"type":"__customFromThinkProvider"}""");

        using (new AssertionScope())
        {
            customFromThinkProvider.Should().BeNull();
            unhandled!.Raw.Should().Contain("__customFromThinkProvider");
        }
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
