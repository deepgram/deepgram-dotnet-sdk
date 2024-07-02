// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Listen.v1.REST;

public record Intent
{
    /// <summary>
    /// <see href="https://developers.deepgram.com/reference/audio-intelligence-apis#intent-recognition"/>
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("intent")]
    public string? Intention { get; set; }

    /// <summary>
    /// <see href="https://developers.deepgram.com/reference/audio-intelligence-apis#intent-recognition"/>
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("confidence_score")]
    public double? ConfidenceScore { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return Regex.Unescape(JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions));
    }
}



