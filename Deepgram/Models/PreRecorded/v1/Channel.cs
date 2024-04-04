// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.PreRecorded.v1;

public record Channel
{
    /// <summary>
    /// ReadOnlyList of <see cref="Alternative"/> objects.
    /// </summary>
    [JsonPropertyName("alternatives")]
    public IReadOnlyList<Alternative>? Alternatives { get; set; }

    /// <summary>
    /// BCP-47 language tag for the dominant language identified in the channel.
    /// </summary>
    /// <remark>Only available in pre-recorded requests</remark>
    [JsonPropertyName("detected_language")]
    public string? DetectedLanguage { get; set; }

    /// <summary>
    /// Language confidence score for the dominant language identified in the channel.
    /// </summary>
    [JsonPropertyName("language_confidence")]
    public double? LanguageConfidence { get; set; }

    /// <summary>
    /// ReadOnlyList of Search objects.
    /// </summary>
    [JsonPropertyName("search")]
    public IReadOnlyList<Search>? Search { get; set; }
}
