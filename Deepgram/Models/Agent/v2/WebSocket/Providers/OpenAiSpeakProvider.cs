// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Agent.v2.WebSocket;

/// <summary>
/// OpenAI text-to-speech provider. Assign to Speak.Provider.
///
/// Required by the API: <c>Type</c> (defaulted), <c>Model</c> and <c>Voice</c>. Unset properties
/// are omitted from the payload, so a missing required field is rejected by the server rather than
/// caught locally.
/// </summary>
public record OpenAiSpeakProvider
{
    /// <summary>
    /// Provider type. Always "open_ai".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    public string? Type { get; set; } = "open_ai";

    /// <summary>
    /// The REST API version for the OpenAI text-to-speech API. Accepted value: "v1".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>
    /// OpenAI TTS model. Either "tts-1" or "tts-1-hd".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// OpenAI voice. One of "alloy", "echo", "fable", "onyx", "nova" or "shimmer".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("voice")]
    public string? Voice { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
