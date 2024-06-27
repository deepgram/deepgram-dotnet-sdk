// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Listen.v1.WebSocket;

public record Channel
{
    /// <summary>
    /// ReadOnlyList of <see cref="Alternative"/> objects.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("alternatives")]
    public IReadOnlyList<Alternative>? Alternatives { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return Regex.Unescape(JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions));
    }
}
