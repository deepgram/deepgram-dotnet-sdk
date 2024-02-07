// Copyright 2021-2023 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.PreRecorded.v1;

public record Word
{
    /// <summary>
    /// Value between 0 and 1 indicating the model's relative confidence in this word.
    /// </summary>

    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    /// <summary>
    /// Offset in seconds from the start of the audio to where the spoken word ends.
    /// </summary>
    [JsonPropertyName("end")]
    public decimal? End { get; set; }

    /// <summary>
    /// Punctuated version of the word
    /// </summary>
    [JsonPropertyName("punctuated_word")]
    public string? PunctuatedWord { get; set; }

    /// <summary>
    /// Punctuated version of the word
    /// </summary>
    [JsonPropertyName("sentiment")]
    public string? Sentiment { get; set; }

    /// <summary>
    /// Punctuated version of the word
    /// </summary>
    [JsonPropertyName("sentiment_score")]
    public double? SentimentScore { get; set; }

    /// <summary>
    /// Integer indicating the speaker who is saying the word being processed.
    /// </summary>
    [JsonPropertyName("speaker")]
    public int? Speaker { get; set; }

    /// <summary>
    /// Value between 0 and 1 indicating the model's relative confidence in its choice of speaker.
    /// </summary>
    [JsonPropertyName("speaker_confidence")]
    public double? SpeakerConfidence { get; set; }

    /// <summary>
    /// Offset in seconds from the start of the audio to where the spoken word starts.
    /// </summary>
    [JsonPropertyName("start")]
    public decimal? Start { get; set; }

    /// <summary>
    /// Distinct word heard by the model.
    /// </summary>
    [JsonPropertyName("word")]
    public string? HeardWord { get; set; }
}

