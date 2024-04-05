// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Analyze.v1;

public record IntentsInfo
{
    /// <summary>
    /// Input tokens used in the model.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("input_tokens")]
    public int? InputTokens { get; set; }

    /// <summary>
    /// Model UUID.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("model_uuid")]
    public string? ModelUuid { get; set; }

    /// <summary>
    /// Output tokens used in the model.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("output_tokens")]
    public int? OutputTokens { get; set; }
}


