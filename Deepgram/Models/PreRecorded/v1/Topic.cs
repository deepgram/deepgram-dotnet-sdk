// Copyright 2021-2023 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.PreRecorded.v1;

public record Topic
{
    /// <summary>
    /// Value between 0 and 1 indicating the model's relative confidence in this topic.
    /// </summary>
    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("topic")]
    public string Text;
}

