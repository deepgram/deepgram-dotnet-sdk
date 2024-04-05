// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Analyze.v1;

public record Topic
{
    /// <summary>
    /// Value between 0 and 1 indicating the model's relative confidence in this topic.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    /// <summary>
    /// The discover topic.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("topic")]
    public string Text;
}

