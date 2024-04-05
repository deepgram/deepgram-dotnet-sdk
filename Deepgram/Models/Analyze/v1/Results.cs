// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Analyze.v1;

public record Results
{
    /// <summary>
    /// Intents <see cref="IntentGroup"/>
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("intents")]
    public IntentGroup? Intents { get; set; }

    /// <summary>
    /// Sentiments <see cref="SentimentGroup"/>
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("sentiments")]
    public SentimentGroup? Sentiments { get; set; }

    /// <summary>
    /// Transcription Summary <see cref="TranscriptionSummary"/>
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("summary")]
    public Summary? Summary { get; set; }

    /// <summary>
    /// Summary of the topics <see cref="TopicGroup"/>
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("topics")]
    public TopicGroup? Topics { get; set; }
}

