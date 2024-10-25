// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Common.v2.WebSocket;

public record CloseResponse
{
    /// <summary>
    /// Close event type.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WebSocketType? Type { get; set; } = WebSocketType.Close;

    /// <summary>
    /// Copy method to copy the object
    /// </summary>
    public void Copy(CloseResponse other)
    {
        if (other is null)
        {
            return;
        }

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
