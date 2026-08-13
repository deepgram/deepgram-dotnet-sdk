// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.Speak.WebSocket;

/// <summary>
/// Barge-in message for the Deepgram Flux (v2 speak) API. Cancels the currently active turn.
/// Stopping local audio playback is the caller's responsibility — Interrupt is for reconciling
/// the server's record of what was spoken, not for stopping audio.
///
/// Including <see cref="PlaybackOffset"/> lets the server compute the text_spoken/text_remaining
/// split reported on SpeechInterrupted; omitting it still cancels the turn, but that split comes
/// back empty. An Interrupt the server cannot act on (no audio generated yet, an earlier interrupt
/// still in flight, a non-advancing offset, or no active turn) is answered with a Warning instead
/// of SpeechInterrupted.
/// <see href="https://developers.deepgram.com/docs/flux-tts/interrupt-handling"/>
/// </summary>
public class InterruptSchema
{
    /// <summary>
    /// Message type. Always "Interrupt".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    public string? Type { get; set; } = "Interrupt";

    /// <summary>
    /// Playback position when the barge-in occurred. See <see cref="PlaybackOffset"/> for the
    /// cumulative/advancing semantics. Optional — omit only if you don't need the spoken-text split.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("playback_offset")]
    public PlaybackOffset? PlaybackOffset { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
