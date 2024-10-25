// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Speak.v2.WebSocket;

public record AudioResponse : IDisposable
{
    /// <summary>
    /// The type of speak response, defaults to Audio.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SpeakType? Type { get; set; } = SpeakType.Audio;

    /// <summary>
    /// A stream of the audio file
    /// </summary>
    public MemoryStream? Stream { get; set; }

    // NOTE: There isn't a ToString() function because this will cause an odd Exception to be thrown:
    // InvalidOperationException: "Timeouts are not supported on this stream."

    public void Dispose()
    {
        Stream?.Dispose();
        Stream = null;
    }
}
