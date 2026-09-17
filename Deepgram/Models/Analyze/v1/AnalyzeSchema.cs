// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Analyze.v1;

public class AnalyzeSchema
{
    /// <summary>
    /// Callback URL to provide if you would like your submitted text to be processed.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("callback")]
    public string? CallBack { get; set; }

    /// <summary>
    /// Enable a callback method
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("callback_method")]
    public string? CallbackMethod { get; set; }

    /// <summary>
    /// Define custom intents.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("custom_intent")]
    public List<string>? CustomIntent { get; set; }

    /// <summary>
    /// When strict, the model will only return intents submitted using the custom_intent param. When extended, the model will return its own detected intents in addition to those submitted using the custom_intent param. Default: extended
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("custom_intent_mode")]
    public string? CustomIntentMode { get; set; }

    /// <summary>
    /// Define custom topics.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("custom_topic")]
    public List<string>? CustomTopic { get; set; }

    /// <summary>
    /// When strict, the model will only return topics submitted using the custom_topic param. When extended, the model will return its own detected topics in addition to those submitted using the custom_topic param. Default: extended
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("custom_topic_mode")]
    public string? CustomTopicMode { get; set; }

    /// <summary>
    /// Recognizes speaker intent throughout an entire input text. Returns a list of text segments and the intents found within each segment
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("intents")]
    public bool? Intents { get; set; }

    /// <summary>
    /// Required: en. The language of your input text. (Only English language is supported at this time)
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>
    /// Recognizes the sentiment of the entire input text and detects a shift in sentiment throughout. Returns a list of text segments and the sentiment found within each segment.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("sentiment")]
    public bool? Sentiment { get; set; }

    /// <summary>
    /// Provides a brief summary of the input text.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("summarize")]
    public bool? Summarize { get; set; }

    /// <summary>
    /// Detects topics throughout an entire input text. Returns a list of text segments and the topics found within each segment.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("topics")]
    public bool? Topics { get; set; }

    /// <summary>
    /// Override ToString method to serialize the object
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonSerializeOptions.DefaultOptions);
    }
}

