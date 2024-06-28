// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Listen.v1.REST;

public record Word
{
    /// <summary>
    /// Value between 0 and 1 indicating the model's relative confidence in this word.
    /// </summary>

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    /// <summary>
    /// Offset in seconds from the start of the audio to where the spoken word ends.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("end")]
    public decimal? End { get; set; }

    /// <summary>
    /// Punctuated version of the word
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("punctuated_word")]
    public string? PunctuatedWord { get; set; }

    /// <summary>
    /// Punctuated version of the word
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("sentiment")]
    public string? Sentiment { get; set; }

    /// <summary>
    /// Punctuated version of the word
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("sentiment_score")]
    public double? SentimentScore { get; set; }

    /// <summary>
    /// Integer indicating the speaker who is saying the word being processed.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("speaker")]
    public int? Speaker { get; set; }

    /// <summary>
    /// Value between 0 and 1 indicating the model's relative confidence in its choice of speaker.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("speaker_confidence")]
    public double? SpeakerConfidence { get; set; }

    /// <summary>
    /// Offset in seconds from the start of the audio to where the spoken word starts.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("start")]
    public decimal? Start { get; set; }

    /// <summary>
    /// Distinct word heard by the model.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("word")]
    public string? HeardWord { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return Regex.Unescape(JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions));
    }
}

