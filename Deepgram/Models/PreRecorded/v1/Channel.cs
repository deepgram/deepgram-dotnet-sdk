// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.PreRecorded.v1;

public record Channel
{
    /// <summary>
    /// ReadOnlyList of <see cref="Alternative"/> objects.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("alternatives")]
    public IReadOnlyList<Alternative>? Alternatives { get; set; }

    /// <summary>
    /// BCP-47 language tag for the dominant language identified in the channel.
    /// </summary>
    /// <remark>Only available in pre-recorded requests</remark>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("detected_language")]
    public string? DetectedLanguage { get; set; }

    /// <summary>
    /// Language confidence score for the dominant language identified in the channel.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("language_confidence")]
    public double? LanguageConfidence { get; set; }

    /// <summary>
    /// ReadOnlyList of Search objects.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("search")]
    public IReadOnlyList<Search>? Search { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}
