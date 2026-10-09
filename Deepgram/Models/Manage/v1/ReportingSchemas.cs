// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Manage.v1;

/// <summary>
/// Filters and groups project usage reporting.
/// </summary>
public class UsageBreakdownSchema
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("start")]
    public DateTime? Start { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("end")]
    public DateTime? End { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("grouping")]
    public string? Grouping { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("accessor")]
    public string? Accessor { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("tag")]
    public string? Tag { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("deployment")]
    public string? Deployment { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("endpoint")]
    public string? Endpoint { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("method")]
    public string? Method { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("alternatives")] public bool? Alternatives { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("callback_method")] public bool? CallbackMethod { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("callback")] public bool? Callback { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("channels")] public bool? Channels { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("custom_intent_mode")] public bool? CustomIntentMode { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("custom_intent")] public bool? CustomIntent { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("custom_topic_mode")] public bool? CustomTopicMode { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("custom_topic")] public bool? CustomTopic { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("detect_entities")] public bool? DetectEntities { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("detect_language")] public bool? DetectLanguage { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("diarize")] public bool? Diarize { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("dictation")] public bool? Dictation { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("encoding")] public bool? Encoding { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("extra")] public bool? Extra { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("filler_words")] public bool? FillerWords { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("intents")] public bool? Intents { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("keyterm")] public bool? Keyterm { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("keywords")] public bool? Keywords { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("language")] public bool? Language { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("measurements")] public bool? Measurements { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("multichannel")] public bool? Multichannel { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("numerals")] public bool? Numerals { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("paragraphs")] public bool? Paragraphs { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("profanity_filter")] public bool? ProfanityFilter { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("punctuate")] public bool? Punctuate { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("redact")] public bool? Redact { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("replace")] public bool? Replace { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("sample_rate")] public bool? SampleRate { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("search")] public bool? Search { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("sentiment")] public bool? Sentiment { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("smart_format")] public bool? SmartFormat { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("summarize")] public bool? Summarize { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("topics")] public bool? Topics { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("utt_split")] public bool? UttSplit { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("utterances")] public bool? Utterances { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("version")] public bool? Version { get; set; }
}

/// <summary>
/// Filters and groups project billing reporting.
/// </summary>
public class BillingBreakdownSchema
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("start")] public DateTime? Start { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("end")] public DateTime? End { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("accessor")] public string? Accessor { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("tag")] public string? Tag { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("line_item")] public string? LineItem { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("deployment")] public string? Deployment { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("grouping")]
    public List<string>? Grouping { get; set; }
}

/// <summary>
/// Limits the number of project purchases returned.
/// </summary>
public class PurchasesSchema
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("limit")]
    public int? Limit { get; set; }
}

/// <summary>
/// Filters billing field values by date range.
/// </summary>
public class BillingFieldsSchema
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("start")] public DateTime? Start { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] [JsonPropertyName("end")] public DateTime? End { get; set; }
}
