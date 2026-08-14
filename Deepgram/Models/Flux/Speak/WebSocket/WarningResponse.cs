// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.Speak.WebSocket;

/// <summary>
/// Informational message from the Deepgram Flux (v2 speak) API; synthesis continues and
/// the connection is unaffected. Known codes include NO_ACTIVE_SPEECH (a speech-scoped message
/// arrived with no active turn), SYNTHESIS_RETRYING (a synthesis request failed and is being
/// retried), NO_AUDIO_GENERATED, INTERRUPT_IN_PROGRESS, and INVALID_INTERRUPT_OFFSET (all three
/// mean an Interrupt could not be acted on). The code set is open — do not assume it is exhaustive.
/// </summary>
public record WarningResponse
{
    /// <summary>
    /// Warning event type.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SpeakType? Type { get; set; } = SpeakType.Warning;

    /// <summary>
    /// Warning code identifying the condition, in SCREAMING_SNAKE_CASE.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>
    /// A human-readable description of the warning.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
