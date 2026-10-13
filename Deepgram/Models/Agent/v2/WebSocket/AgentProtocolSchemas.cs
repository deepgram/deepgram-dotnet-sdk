// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Agent.v2.WebSocket;

/// <summary>
/// Updates the active speech-to-text configuration during an Agent session.
/// </summary>
public class AgentUpdateListenSchema
{
    [JsonPropertyName("type")]
    public string Type { get; } = AgentClientTypes.UpdateListen;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("listen")]
    public Listen? Listen { get; set; }

    public override string ToString() => JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
}

/// <summary>
/// Updates the active Think configuration during an Agent session. The protocol accepts either a
/// single <see cref="Think"/> object or an array of Think configurations.
/// </summary>
public class AgentUpdateThinkSchema
{
    [JsonPropertyName("type")]
    public string Type { get; } = AgentClientTypes.UpdateThink;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("think")]
    public object? Think { get; set; }

    public override string ToString() => JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
}

/// <summary>
/// Updates the active text-to-speech configuration during an Agent session. The protocol accepts
/// either a single <see cref="Speak"/> object or an array of Speak configurations.
/// </summary>
public class AgentUpdateSpeakSchema
{
    [JsonPropertyName("type")]
    public string Type { get; } = AgentClientTypes.UpdateSpeak;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("speak")]
    public object? Speak { get; set; }

    public override string ToString()
    {
        // The session Settings model nests multi-provider TTS under speak.speak. UpdateSpeak
        // accepts that provider list directly as its speak payload.
        if (Speak is Speak { SpeakProviders: not null } speak)
        {
            return JsonSerializer.Serialize(new { type = Type, speak = speak.SpeakProviders }, JsonSerializeOptions.DefaultOptions);
        }

        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}

/// <summary>
/// Updates the Agent system prompt during an active session.
/// </summary>
public class AgentUpdatePromptSchema
{
    [JsonPropertyName("type")]
    public string Type { get; } = AgentClientTypes.UpdatePrompt;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("prompt")]
    public string? Prompt { get; set; }

    public override string ToString() => JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
}

/// <summary>
/// Immediately injects speech for the Agent to deliver.
/// </summary>
public class AgentInjectAgentMessageSchema
{
    [JsonPropertyName("type")]
    public string Type { get; } = AgentClientTypes.InjectAgentMessage;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("behavior")]
    public string? Behavior { get; set; }

    public override string ToString() => JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
}

/// <summary>
/// Returns the result of a client-side function invocation to the Agent.
/// </summary>
public class AgentFunctionCallResponseSchema
{
    [JsonPropertyName("type")]
    public string Type { get; } = AgentClientTypes.FunctionCallResponse;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    public override string ToString() => JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
}

/// <summary>
/// Sends an arbitrary JSON value to a custom Think provider.
/// </summary>
public class AgentCustomToThinkProviderSchema
{
    [JsonPropertyName("type")]
    public string Type { get; } = AgentClientTypes.CustomToThinkProvider;

    [JsonPropertyName("content")]
    public JsonElement Content { get; set; }

    public override string ToString() => JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
}

/// <summary>
/// Ends the current user turn when the Agent uses a Flux STT listen provider.
/// </summary>
public class AgentForceEndTurnSchema
{
    [JsonPropertyName("type")]
    public string Type { get; } = AgentClientTypes.ForceEndTurn;

    public override string ToString() => JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
}
