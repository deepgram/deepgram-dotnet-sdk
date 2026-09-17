// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.Speak.WebSocket;

/// <summary>
/// Per-turn billing and timing for an interrupted turn, nested inside
/// <see cref="SpeechInterruptedResponse"/>. Same shape as the metadata carried by
/// <see cref="SpeechMetadataResponse"/>.
/// </summary>
public record SpeechInterruptedMetadata
{
    /// <summary>
    /// Server-assigned identifier of the interrupted turn.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("speech_id")]
    public string? SpeechId { get; set; }

    /// <summary>
    /// Total audio duration produced for this turn before it was cut off, in milliseconds.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("audio_duration_ms")]
    public int? AudioDurationMs { get; set; }

    /// <summary>
    /// Raw input character count for this turn, before text normalization.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("input_character_count")]
    public int? InputCharacterCount { get; set; }

    /// <summary>
    /// Billable character count for this turn. Always less than or equal to InputCharacterCount.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("billable_character_count")]
    public int? BillableCharacterCount { get; set; }

    /// <summary>
    /// Controls applied during the turn before it was interrupted.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("controls_applied")]
    public ControlsApplied? ControlsApplied { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
