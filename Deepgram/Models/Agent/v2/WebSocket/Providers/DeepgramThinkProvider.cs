// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Agent.v2.WebSocket;

/// <summary>
/// Deepgram think provider. Assign to Think.Provider.
/// </summary>
public record DeepgramThinkProvider
{
    /// <summary>
    /// Provider type. Always "deepgram".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    public string? Type { get; set; } = "deepgram";

    /// <summary>
    /// Deepgram model to use, such as "think-fast".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// API version for the Deepgram think provider. Defaults to <c>v1</c>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("version")]
    public string? Version { get; set; } = "v1";

    /// <summary>
    /// Sampling temperature from 0 through 2.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    /// <summary>
    /// Reasoning effort. With <c>phonellm-mini-alpha-2-fp8</c>, use <c>none</c> or leave unset
    /// so reasoning text is not spoken to the caller.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("reasoning_mode")]
    public string? ReasoningMode { get; set; }

    /// <summary>
    /// With <c>think-fast</c>, opt out of Deepgram's spoken-style prompt prefix.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("voice_prompt_opt_out")]
    public bool? VoicePromptOptOut { get; set; }

    public override string ToString() => JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
}
