// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.WebSocket;

/// <summary>
/// (PREVIEW) A simple type-only control message for the Deepgram Flux (v2 listen) API.
/// Flux accepts exactly one type-only control message: CloseStream. (KeepAlive and Finalize
/// are v1-only and are rejected by the v2 endpoint.)
/// </summary>
public class ControlMessage(string text)
{
    /// <summary>
    /// Gets or sets the type of control message.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("type")]
    public string? Type { get; set; } = text;

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return Regex.Unescape(JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions));
    }
}
