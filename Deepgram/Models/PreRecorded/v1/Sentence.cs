// Copyright 2021-2023 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.PreRecorded.v1;

public record Sentence
{
    /// <summary>
    /// Text transcript of the sentence.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Offset in seconds from the start of the audio to where the sentence starts.
    /// </summary>
    [JsonPropertyName("start")]
    public decimal? Start { get; set; }

    /// <summary>
    /// Offset in seconds from the start of the audio to where the sentence ends.
    /// </summary>
    [JsonPropertyName("end")]
    public decimal? End { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("sentiment")]
    public string? Sentiment { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("sentiment_score")]
    public double? SentimentScore { get; set; }
}

