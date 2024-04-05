// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.PreRecorded.v1;

public record SentimentGroup
{
    /// <summary>
    /// Segments of the audio
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("segments")]
    public IReadOnlyList<Segment>? Segments { get; set; }

    /// <summary>
    /// Sentiment of the segment
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("average")]
    public Average? Average { get; set; }
}


