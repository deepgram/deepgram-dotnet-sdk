// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Agent.v2.WebSocket;

/// <summary>
/// ElevenLabs text-to-speech provider. Assign to Speak.Provider.
/// </summary>
public record ElevenLabsSpeakProvider
{
    /// <summary>
    /// Provider type. Always "eleven_labs".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    public string? Type { get; set; } = "eleven_labs";

    /// <summary>
    /// The REST API version for the ElevenLabs text-to-speech API. Accepted value: "v1".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>
    /// ElevenLabs model ID, e.g. "eleven_turbo_v2_5", "eleven_monolingual_v1"
    /// or "eleven_multilingual_v2".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("model_id")]
    public string? ModelId { get; set; }

    /// <summary>
    /// Optional language to use, e.g. "en-US". Corresponds to the language_code
    /// parameter in the ElevenLabs API.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>
    /// Deprecated. Use Language instead.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("language_code")]
    public string? LanguageCode { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
