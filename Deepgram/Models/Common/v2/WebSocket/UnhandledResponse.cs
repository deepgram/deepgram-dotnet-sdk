// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Common.v2.WebSocket;

public record UnhandledResponse 
{
    /// <summary>
    /// Raw JSON
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("raw")]
    public string? Raw { get; set; } = "";

    /// <summary>
    /// Unhandled event type.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WebSocketType? Type { get; set; } = WebSocketType.Unhandled;

    /// <summary>
    /// Copy method to copy the object
    /// </summary>
    public void Copy(UnhandledResponse other)
    {
        if (other == null)
        {
            return;
        }

        Raw = other.Raw;
        Type = other.Type;
    }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return Regex.Unescape(JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions));
    }
}
