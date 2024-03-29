﻿// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Analyze.v1;

public class AnalyzeSchema
{
    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("callback")]
    public string? CallBack { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("callback_method")]
    public string? CallbackMethod { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("custom_intent")]
    public List<string>? CustomIntent { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("custom_intent_mode")]
    public string? CustomIntentMode { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("custom_topic")]
    public List<string>? CustomTopic { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("custom_topic_mode")]
    public string? CustomTopicMode { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("intents")]
    public bool? Intents { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("sentiment")]
    public bool? Sentiment { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("summarize")]
    public bool? Summarize { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("topics")]
    public bool? Topics { get; set; }
}

