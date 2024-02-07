// Copyright 2021-2023 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.PreRecorded.v1;

public record Intent
{
    /// <summary>
    /// <see href="https://developers.deepgram.com/reference/audio-intelligence-apis#intent-recognition"/>
    /// </summary>
    [JsonPropertyName("intent")]
    public string? Intention { get; set; }

    /// <summary>
    /// <see href="https://developers.deepgram.com/reference/audio-intelligence-apis#intent-recognition"/>
    /// </summary>
    [JsonPropertyName("confidence_score")]
    public double? ConfidenceScore { get; set; }
}



