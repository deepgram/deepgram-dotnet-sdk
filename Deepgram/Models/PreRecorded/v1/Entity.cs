// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.PreRecorded.v1;

public record Entity
{
    /// <summary>
    /// This is the type of the entity
    /// </summary>
    /// <remarks>e.g. DATE, PER, ORG, etc.</remarks>
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// This is the value of the detected entity.
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    /// <summary>
    /// Starting index of the entities words within the transcript.
    /// </summary>
    [JsonPropertyName("start_word")]
    public int? StartWord { get; set; }

    /// <summary>
    /// Ending index of the entities words within the transcript.
    /// </summary>
    [JsonPropertyName("end_word")]
    public int? EndWord { get; set; }

    /// <summary>
    /// Value between 0 and 1 indicating the model's relative confidence in this detected entity.
    /// </summary>
    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }
}
