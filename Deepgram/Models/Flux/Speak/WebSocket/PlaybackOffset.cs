// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.Speak.WebSocket;

/// <summary>
/// A playback position, in milliseconds, measured from the start of the session's audio (not the
/// current turn). Sent on <see cref="InterruptSchema"/> so the server can compute the
/// text_spoken/text_remaining split on <see cref="SpeechInterruptedResponse"/>.
/// </summary>
public class PlaybackOffset
{
    /// <summary>
    /// Creates a playback offset of the given value, in milliseconds.
    /// </summary>
    public PlaybackOffset(long valueMs)
    {
        Value = valueMs;
    }

    /// <summary>
    /// Offset unit. Always "time_ms".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    public string? Type { get; set; } = "time_ms";

    /// <summary>
    /// Milliseconds of audio played, cumulative from the start of the session. Each Interrupt's
    /// offset must advance past the position established by the previous Interrupt.
    /// </summary>
    [JsonPropertyName("value")]
    public long Value { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
