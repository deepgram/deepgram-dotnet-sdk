// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram;

/// <summary>
/// Formats inline Flux TTS voice controls. Server validation remains authoritative.
/// </summary>
public static class FluxSpeakTextControls
{
    /// <summary>
    /// Formats an IPA pronunciation marker for Flux TTS REST or WebSocket text.
    /// </summary>
    public static string Pronunciation(string word, string ipa)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(word);
        ArgumentException.ThrowIfNullOrWhiteSpace(ipa);
        var payload = JsonSerializer.Serialize(new { word, pronounce = ipa });
        return $"\\{payload[..^1]}\\}}";
    }

    /// <summary>
    /// Formats a REST-only pause marker. Do not send pause markers over Flux TTS WebSocket.
    /// </summary>
    public static string Pause(TimeSpan duration)
    {
        if (duration < TimeSpan.FromMilliseconds(500) || duration > TimeSpan.FromMilliseconds(3000) || duration.TotalMilliseconds % 1 != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Pause duration must be a whole number of milliseconds between 500 and 3000.");
        }

        return $"\\{{pause:{duration.TotalMilliseconds:0}ms\\}}";
    }
}
