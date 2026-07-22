// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.Speak.WebSocket;

/// <summary>
/// (PREVIEW) A binary audio chunk received from the Deepgram Flux (v2 speak) API, in the encoding
/// and sample rate requested on the connection. Correlate audio with a turn via the surrounding
/// SpeechStarted / Flushed / SpeechMetadata events; audio chunks carry no envelope of their own.
/// </summary>
public record AudioResponse : IDisposable
{
    /// <summary>
    /// The type of response, defaults to Audio.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SpeakType? Type { get; set; } = SpeakType.Audio;

    /// <summary>
    /// A stream of the raw audio bytes for this chunk.
    /// </summary>
    public MemoryStream? Stream { get; set; }

    // NOTE: There isn't a ToString() function because this will cause an odd Exception to be thrown:
    // InvalidOperationException: "Timeouts are not supported on this stream."

    public void Dispose()
    {
        Stream?.Dispose();
    }
}
