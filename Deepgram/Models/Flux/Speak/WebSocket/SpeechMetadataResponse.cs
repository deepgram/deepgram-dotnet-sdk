// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.Speak.WebSocket;

/// <summary>
/// (PREVIEW) Emitted by the Deepgram Flux (v2 speak) API at turn boundaries (manual Flush), after
/// all audio for the turn has been sent. Reports billing and timing for the completed turn.
/// </summary>
public record SpeechMetadataResponse
{
    /// <summary>
    /// SpeechMetadata event type.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SpeakType? Type { get; set; } = SpeakType.SpeechMetadata;

    /// <summary>
    /// Server-assigned turn identifier.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("speech_id")]
    public string? SpeechId { get; set; }

    /// <summary>
    /// Total audio duration produced for this turn, in milliseconds.
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
    /// Billable character count for this turn (input character count with stripped control
    /// characters removed). Always less than or equal to InputCharacterCount.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("billable_character_count")]
    public int? BillableCharacterCount { get; set; }

    /// <summary>
    /// Controls applied during the turn. Always reports 0 during Early Access.
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
