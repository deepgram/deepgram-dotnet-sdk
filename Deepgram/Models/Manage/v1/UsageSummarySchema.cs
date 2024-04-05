// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Manage.v1;

public class UsageSummarySchema
{
    /// <summary>
    /// List of Accessors, Limits results to the given API key(s)
    /// </summary>
    [JsonPropertyName("accessor")]
    public List<string>? Accessor { get; set; }

    /// <summary>
    /// Limits results to requests that include the alternatives feature.
    /// </summary>
    [JsonPropertyName("alternatives")]
    public bool? Alternatives { get; set; }

    /// <summary>
    /// Limits results to requests fulfilled in either Deepgram hosted cloud or your onprem deployment environment.
    /// multiple deployments can be included in list
    /// </summary>
    [JsonPropertyName("detect_entities")]
    public bool? DetectEntities { get; set; }

    /// <summary>
    /// Limits results to requests that include the topic detection feature.
    /// </summary>
    [JsonPropertyName("detect_topics")]
    public bool? DetectTopics { get; set; }

    /// <summary>
    /// Limits results to requests that include the diarize feature.
    /// </summary>
    [JsonPropertyName("diarize")]
    public bool? Diarize { get; set; }

    /// <summary>
    /// End date of the requested date range. Formats accepted are YYYY-MM-DD, YYYY-MM-DDTHH:MM:SS, or YYYY-MM-DDTHH:MM:SS+HH:MM.
    /// </summary>
    [JsonPropertyName("end")]
    public DateTime? End { get; set; }

    /// <summary>
    /// Limits results to requests that include the interim_results feature.
    /// </summary>
    [JsonPropertyName("interim_results")]
    public bool? InterimResults { get; set; }

    /// <summary>
    /// Limits results to requests that include the keywords feature.
    /// </summary>
    [JsonPropertyName("keywords")]
    public bool? Keywords { get; set; }

    /// <summary>
    /// Permitted values "sync" | "async" | "streaming"
    /// <see cref="RequestMethod">
    /// </summary>
    [JsonPropertyName("method")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RequestMethod? Method { get; set; }

    /// <summary>
    /// Limits results to requests run with the specified model UUID applied.
    /// </summary>
    [JsonPropertyName("model")]
    public List<string>? Model { get; set; }

    /// <summary>
    /// Limits results to requests that include the multichannel feature.
    /// </summary>
    [JsonPropertyName("multichannel")]
    public bool? MultiChannel { get; set; }

    /// <summary>
    /// Limits results to requests that include the ner feature.
    /// </summary>
    [JsonPropertyName("ner")]
    public bool? Ner { get; set; }

    /// <summary>
    /// Limits results to requests that include the numerals feature.
    /// </summary>
    [JsonPropertyName("numerals")]
    public bool? Numerals { get; set; }

    /// <summary>
    /// Is the Number Feature being used
    /// </summary>
    [JsonPropertyName("numbers")]
    public bool? Numbers { get; set; }

    /// <summary>
    /// Is the Paragraphs Feature being used
    /// </summary>
    [JsonPropertyName("paragraphs")]
    public bool? Paragraphs { get; set; }

    /// <summary>
    /// Limits results to requests that include the profanity_filter feature.
    /// </summary>
    [JsonPropertyName("profanity_filter")]
    public bool? ProfanityFilter { get; set; }

    /// <summary>
    /// Limits results to requests that include the punctuate feature.
    /// </summary>
    [JsonPropertyName("punctuate")]
    public bool? Punctuate { get; set; }

    /// <summary>
    /// Limits results to requests that include the redact feature.
    /// </summary>
    [JsonPropertyName("redact")]
    public bool? Redact { get; set; }

    /// <summary>
    /// Limits results to requests that include the replace feature.
    /// </summary>
    [JsonPropertyName("replace")]
    public bool? Replace { get; set; }

    /// <summary>
    /// Limits results to requests that include the search feature.
    /// </summary>
    [JsonPropertyName("search")]
    public bool? Search { get; set; }

    /// <summary>
    /// Is the Sentiment Feature being used
    /// </summary>
    [JsonPropertyName("sentiment")]
    public bool? Sentiment { get; set; }

    /// <summary>
    /// Limits results to requests that include the search feature.
    /// </summary>
    [JsonPropertyName("sentiment_threshold")]
    public double? SentimentThreshold { get; set; }


    /// <summary>
    /// Is the SmartFormat Feature being used
    /// </summary>
    [JsonPropertyName("smart_format")]
    public bool? SmartFormat { get; set; }

    /// <summary>
    /// Start date of the requested date range. Formats accepted are YYYY-MM-DD, YYYY-MM-DDTHH:MM:SS, or YYYY-MM-DDTHH:MM:SS+HH:MM.
    /// </summary>
    [JsonPropertyName("start")]
    public DateTime? Start { get; set; }

    /// <summary>
    /// Is the Summarize Feature being used
    /// </summary>
    [JsonPropertyName("summarize")]
    public bool? Summarize { get; set; }

    /// <summary>
    /// List of Tags, Limits results to requests associated with the specified tag(s). 
    /// </summary>
    [JsonPropertyName("tag")]
    public List<string>? Tag { get; set; }

    /// <summary>
    /// Is the Translate Feature being used
    /// </summary>
    [JsonPropertyName("translate")]
    public bool? translate { get; set; }

    /// <summary>
    /// Limits results to requests that include the utterances feature.
    /// </summary>
    [JsonPropertyName("utterances")]
    public bool? Utterances { get; set; }

    /// <summary>
    /// Is the Utterance Split Feature being used
    /// </summary>
    [JsonPropertyName("utt_split")]
    public bool? UttSplit { get; set; }
}
