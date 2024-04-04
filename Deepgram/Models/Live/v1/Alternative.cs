// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Live.v1;

public record Alternative
{
    /// <summary>
    /// Single-string transcript containing what the model hears in this channel of audio.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("transcript")]
    public string Transcript { get; set; }
    /// <summary>
    /// Value between 0 and 1 indicating the model's relative confidence in this transcript.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    /// <summary>
    /// ReadOnly List of <see cref="Word"/> objects.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("words")]
    public IReadOnlyList<Word> Words { get; set; }
}
