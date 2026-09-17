// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.Speak.WebSocket;

/// <summary>
/// Final server message from the Deepgram Flux (v2 speak) API before the WebSocket
/// closes. Reports cumulative session totals across all turns.
/// </summary>
public record SessionMetadataResponse
{
    /// <summary>
    /// SessionMetadata event type.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SpeakType? Type { get; set; } = SpeakType.SessionMetadata;

    /// <summary>
    /// Cumulative audio duration produced across the session, in milliseconds.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("total_audio_duration_ms")]
    public int? TotalAudioDurationMs { get; set; }

    /// <summary>
    /// Cumulative raw input character count across the session.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("total_input_character_count")]
    public int? TotalInputCharacterCount { get; set; }

    /// <summary>
    /// Cumulative billable character count across the session.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("total_billable_character_count")]
    public int? TotalBillableCharacterCount { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
