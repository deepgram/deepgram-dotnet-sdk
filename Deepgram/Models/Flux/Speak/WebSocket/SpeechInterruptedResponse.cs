// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.Speak.WebSocket;

/// <summary>
/// Emitted by the Deepgram Flux (v2 speak) API in response to a client Interrupt. Reports how much
/// audio actually played and, when the Interrupt carried a playback_offset, the exact split between
/// what the user heard (<see cref="TextSpoken"/>) and what they didn't (<see cref="TextRemaining"/>).
/// The next Speak begins a new turn; Interrupt does not reset the model's conversational state.
/// <see href="https://developers.deepgram.com/docs/flux-tts/interrupt-handling"/>
/// </summary>
public record SpeechInterruptedResponse
{
    /// <summary>
    /// SpeechInterrupted event type.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SpeakType? Type { get; set; } = SpeakType.SpeechInterrupted;

    /// <summary>
    /// Milliseconds of audio actually played before the cut, per the server's own record.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("audio_played_ms")]
    public long? AudioPlayedMs { get; set; }

    /// <summary>
    /// The text the user actually heard before the interrupt. Present only when the Interrupt
    /// carried a playback_offset.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("text_spoken")]
    public string? TextSpoken { get; set; }

    /// <summary>
    /// The text that was cut off and never spoken. Present only when the Interrupt carried a
    /// playback_offset.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("text_remaining")]
    public string? TextRemaining { get; set; }

    /// <summary>
    /// Billing and timing for the interrupted turn.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("metadata")]
    public SpeechInterruptedMetadata? Metadata { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
