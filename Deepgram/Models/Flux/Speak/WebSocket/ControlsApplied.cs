// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.Speak.WebSocket;

/// <summary>
/// Controls applied during a turn, nested inside <see cref="SpeechMetadataResponse"/> and
/// <see cref="SpeechInterruptedMetadata"/>. Inline pronunciation and pause controls are a coming-soon
/// fast-follow, so every count is currently 0.
/// </summary>
public record ControlsApplied
{
    /// <summary>
    /// Pronunciation overrides successfully applied. Currently always 0 (coming-soon fast-follow).
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("pronunciations_applied")]
    public int? PronunciationsApplied { get; set; }

    /// <summary>
    /// Pronunciation entries that triggered a warning (invalid IPA, word too long).
    /// Currently always 0 (coming-soon fast-follow).
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("pronunciation_warnings")]
    public int? PronunciationWarnings { get; set; }

    /// <summary>
    /// Inline pause (break) controls successfully applied. Nullable because older server
    /// responses predate this counter and omit the field entirely.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("breaks_applied")]
    public int? BreaksApplied { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
