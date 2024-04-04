// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.PreRecorded.v1;

public record Segment
{
    /// <summary>
    /// The translated text
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Start time of the segment in seconds
    /// </summary>
    [JsonPropertyName("start_word")]
    public int? StartWord { get; set; }

    /// <summary>
    /// End time of the segment in seconds
    /// </summary>
    [JsonPropertyName("end_word")]
    public int? EndWord { get; set; }

    /// <summary>
    /// Sentiment: positive, negative, neutral
    /// </summary>
    [JsonPropertyName("sentiment")]
    public string? Sentiment { get; set; }

    /// <summary>
    /// Sentiment score
    /// </summary>
    [JsonPropertyName("sentiment_score")]
    public double? SentimentScore { get; set; }

    /// <summary>
    /// Topics discovered in the segment
    /// </summary>
    [JsonPropertyName("topics")]
    public IReadOnlyList<Topic>? Topics { get; set; }

    /// <summary>
    /// Intents discovered in the segment
    /// </summary>
    [JsonPropertyName("intents")]
    public IReadOnlyList<Intent>? Intents { get; set; }
}
