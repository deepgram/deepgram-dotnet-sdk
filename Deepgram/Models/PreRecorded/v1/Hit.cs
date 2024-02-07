// Copyright 2021-2023 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.PreRecorded.v1;

public record Hit
{
    /// <summary>
    /// Value between 0 and 1 that indicates the model's relative confidence in this hit.
    /// </summary>
    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }


    /// <summary>
    /// Offset in seconds from the start of the audio to where the hit ends.
    /// </summary>
    [JsonPropertyName("end")]
    public decimal? End { get; set; }

    /// <summary>
    /// Transcript that corresponds to the time between start and end.
    /// </summary>
    [JsonPropertyName("snippet")]
    public string? Snippet { get; set; }

    /// <summary>
    /// Offset in seconds from the start of the audio to where the hit occurs.
    /// </summary>
    [JsonPropertyName("start")]
    public decimal? Start { get; set; }
}
