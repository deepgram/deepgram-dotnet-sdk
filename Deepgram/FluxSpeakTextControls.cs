// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Text.Encodings.Web;

namespace Deepgram;

/// <summary>
/// Formats inline Flux TTS voice controls. Server validation remains authoritative.
/// </summary>
public static class FluxSpeakTextControls
{
    // Keep IPA as raw UTF-8 (as in the Deepgram docs); quotes and backslashes are still escaped.
    private static readonly JsonSerializerOptions MarkerJsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>
    /// Formats an IPA pronunciation marker for Flux TTS REST or WebSocket text.
    /// </summary>
    public static string Pronunciation(string word, string ipa)
    {
        if (string.IsNullOrWhiteSpace(word)) throw new ArgumentException("Word is required.", nameof(word));
        if (string.IsNullOrWhiteSpace(ipa)) throw new ArgumentException("IPA is required.", nameof(ipa));
        var payload = JsonSerializer.Serialize(new { word, pronounce = ipa }, MarkerJsonOptions);
        return $"\\{payload.Substring(0, payload.Length - 1)}\\}}";
    }

    /// <summary>
    /// Formats a REST-only pause marker. Do not send pause markers over Flux TTS WebSocket.
    /// </summary>
    public static string Pause(TimeSpan duration)
    {
        if (duration < TimeSpan.FromMilliseconds(500) || duration > TimeSpan.FromMilliseconds(3000) || duration.TotalMilliseconds % 100 != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Pause duration must be between 500 and 3000 milliseconds in 100 millisecond increments.");
        }

        return $"\\{{pause:{duration.TotalMilliseconds:0}ms\\}}";
    }
}
