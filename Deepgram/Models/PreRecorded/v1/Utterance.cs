// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.PreRecorded.v1;

public record Utterance
{
    /// <summary>
    /// Audio channel to which the utterance belongs.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("channel")]
    public int? Channel { get; set; }

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
    /// Unique identifier of the utterance
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Sentiment of the word: positive, negative, neutral.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("sentiment")]
    public string? Sentiment { get; set; }

    /// <summary>
    /// Sentiment score of the word.
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
    /// Offset in seconds from the start of the audio to where the spoken word starts.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("start")]
    public decimal? Start { get; set; }

    /// <summary>
    /// Transcript for the audio segment being processed.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("transcript")]
    public string? Transcript { get; set; }

    /// <summary>
    /// Object containing each word in the transcript, along with its start time
    /// and end time(in seconds) from the beginning of the audio stream, and a confidence value.
    /// <see cref="Word"/>
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("words")]
    public List<Word>? Words { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}

