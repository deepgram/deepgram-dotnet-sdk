// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Analyze.v1;

public record Segment
{
    /// <summary>
    /// Transcribed text
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Timestamp of the start of the segment
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("start_word")]
    public int? StartWord { get; set; }

    /// <summary>
    /// Timestamp of the end of the segment
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("end_word")]
    public int? EndWord { get; set; }

    /// <summary>
    /// Sentiment: positive, negative, neutral
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("sentiment")]
    public string? Sentiment { get; set; }

    /// <summary>
    /// Sentiment score
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("sentiment_score")]
    public double? SentimentScore { get; set; }

    /// <summary>
    /// Topics list
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("topics")]
    public IReadOnlyList<Topic>? Topics { get; set; }

    /// <summary>
    /// Summary of the text
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("intents")]
    public IReadOnlyList<Intent>? Intents { get; set; }
}
