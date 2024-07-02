// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Listen.v1.REST;

public record Paragraph
{
    /// <summary>
    /// ReadOnly of Sentence objects.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("sentences")]
    public IReadOnlyList<Sentence>? Sentences { get; set; }

    /// <summary>
    /// Number of words in the paragraph
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("num_words")]
    internal int? NumWords { get; set; }

    /// <summary>
    /// Offset in seconds from the start of the audio to where the paragraph starts.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("start")]
    public decimal? Start { get; set; }

    /// <summary>
    /// Offset in seconds from the start of the audio to where the paragraph ends.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("end")]
    public decimal? End { get; set; }

    /// <summary>
    /// Sentiment of the paragraph
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("sentiment")]
    public string? Sentiment { get; set; }

    /// <summary>
    /// Sentiment score of the paragraph
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("sentiment_score")]
    public double? SentimentScore { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return Regex.Unescape(JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions));
    }
}

