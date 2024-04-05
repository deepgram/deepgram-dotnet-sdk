// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Analyze.v1;

public record Metadata
{
    /// <summary>
    /// Unique identifier for the submitted audio and derived data returned.
    /// </summary>
    [JsonPropertyName("request_id")]
    public string? RequestId { get; set; }

    /// <summary>
    /// Timestamp that indicates when the audio was submitted.
    /// </summary>
    [JsonPropertyName("created")]
    public DateTime? Created { get; set; }

    /// <summary>
    /// Language detected
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>
    /// Intents information
    /// </summary>
    [JsonPropertyName("intents_info")]
    public IntentsInfo? IntentsInfo { get; set; }

    /// <summary>
    /// Sentiment information
    /// </summary>
    [JsonPropertyName("sentiment_info")]
    public SentimentInfo? SentimentInfo { get; set; }

    /// <summary>
    /// Summary information
    /// </summary>
    [JsonPropertyName("summary_info")]
    public SummaryInfo? SummaryInfo { get; set; }

    /// <summary>
    /// Topics information
    /// </summary>
    [JsonPropertyName("topics_info")]
    public TopicsInfo? TopicsInfo { get; set; }
}
