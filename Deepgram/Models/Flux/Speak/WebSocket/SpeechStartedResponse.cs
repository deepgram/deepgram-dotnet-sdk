// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.Speak.WebSocket;

/// <summary>
/// (PREVIEW) Emitted by the Deepgram Flux (v2 speak) API at the start of each new turn, before
/// audio streaming begins. Carries the server-assigned speech_id that identifies the turn. Fires
/// once per turn, not on internal auto-flush boundaries.
/// </summary>
public record SpeechStartedResponse
{
    /// <summary>
    /// SpeechStarted event type.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SpeakType? Type { get; set; } = SpeakType.SpeechStarted;

    /// <summary>
    /// Server-minted identifier for this turn, of the form dg_sp_&lt;12 hex digits&gt;. Informational.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("speech_id")]
    public string? SpeechId { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return Regex.Unescape(JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions));
    }
}
