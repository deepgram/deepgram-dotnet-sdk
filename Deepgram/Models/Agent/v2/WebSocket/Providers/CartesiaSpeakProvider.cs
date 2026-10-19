// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Agent.v2.WebSocket;

/// <summary>
/// Cartesia text-to-speech provider. Assign to Speak.Provider.
///
/// Required by the API: <c>Type</c> (defaulted), <c>ModelId</c> and <c>Voice</c>. Unset properties
/// are omitted from the payload, so a missing required field is rejected by the server rather than
/// caught locally.
/// </summary>
public record CartesiaSpeakProvider
{
    /// <summary>
    /// Provider type. Always "cartesia".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    public string? Type { get; set; } = "cartesia";

    /// <summary>
    /// The API version header for the Cartesia text-to-speech API.
    /// Accepted value: "2025-03-17".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>
    /// Cartesia model ID. Either "sonic-2" or "sonic-multilingual".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("model_id")]
    public string? ModelId { get; set; }

    /// <summary>
    /// Cartesia voice, carrying the voice mode and ID.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("voice")]
    public CartesiaVoice? Voice { get; set; }

    /// <summary>
    /// Cartesia language code.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>
    /// Volume level for Cartesia TTS output. Valid range 0.5 to 2.0.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("volume")]
    public double? Volume { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
