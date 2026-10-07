// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Agent.v2.WebSocket;

using Deepgram.Models.Common.v2.WebSocket;

public enum AgentType
{
    Open = WebSocketType.Open,
    Close = WebSocketType.Close,
    Unhandled = WebSocketType.Unhandled,
    Error = WebSocketType.Error,
    Welcome,
    ConversationText,
    UserStartedSpeaking,
    AgentThinking,
    FunctionCallRequest,
    AgentStartedSpeaking,
    AgentAudioDone,
    Audio,
    InjectionRefused,
    SettingsApplied,
    PromptUpdated,
    SpeakUpdated,
    ListenUpdated,
    ThinkUpdated,
    FunctionCallCancelled,
    FunctionCallResponse,
    CustomFromThinkProvider,
    LatencyReport,
    Warning,
    History,
}

public static class AgentClientTypes
{
    // user message types
    public const string Settings = "Settings";
    public const string UpdatePrompt = "UpdatePrompt";
    public const string UpdateListen = "UpdateListen";
    public const string UpdateThink = "UpdateThink";
    public const string UpdateSpeak = "UpdateSpeak";
    public const string InjectAgentMessage = "InjectAgentMessage";
    public const string InjectUserMessage = "InjectUserMessage";
    public const string FunctionCallResponse = "FunctionCallResponse";
    public const string CustomToThinkProvider = "__customToThinkProvider";
    public const string CustomFromThinkProvider = "__customFromThinkProvider";
    public const string KeepAlive = "KeepAlive";
    public const string ForceEndTurn = "ForceEndTurn";
    public const string Close = "Close";
}
