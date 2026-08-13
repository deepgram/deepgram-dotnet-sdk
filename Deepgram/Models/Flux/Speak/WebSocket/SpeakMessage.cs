// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.Speak.WebSocket;

/// <summary>
/// (PREVIEW) The Speak client message for the Deepgram Flux (v2 speak) API. Sends text to be
/// synthesized into the active turn. Serializes to {"type":"Speak","text":"..."}. The server
/// assigns the turn's speech_id; clients never specify one.
/// </summary>
public class SpeakMessage(string text)
{
    /// <summary>
    /// Message type identifier. Always "Speak".
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    public string? Type { get; set; } = "Speak";

    /// <summary>
    /// The input text to synthesize.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("text")]
    public string? Text { get; set; } = text;

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
