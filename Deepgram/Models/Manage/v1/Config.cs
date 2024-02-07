// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Manage.v1;

public record Config
{
    /// <summary>
    /// Requested maximum number of transcript alternatives to return.
    /// </summary>
    [JsonPropertyName("alternatives")]
    public int? Alternatives { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("callback")]
    public string? Callback { get; set; }

    /// <summary>
    /// Indicates whether topic detection was requested.
    /// </summary>
    [JsonPropertyName("detect_topics")]
    public bool? DetectTopics { get; set; }

    /// <summary>
    /// Indicates whether diarization was requested.
    /// </summary>
    [JsonPropertyName("diarize")]
    public bool? Diarize { get; set; }

    /// <summary>
    /// ReadOnlyList of keywords associated with the request.
    /// </summary>
    [JsonPropertyName("keywords")]
    public IReadOnlyList<string>? Keywords { get; set; }

    /// <summary>
    /// Indicates whether InterimResults was associated with the request.
    /// </summary>
    [JsonPropertyName("interim_results")]
    public bool? InterimResults { get; set; }

    /// <summary>
    /// Language associated with the request.
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>
    /// Model associated with the request.
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// Indicates whether multichannel processing was requested.
    /// </summary>
    [JsonPropertyName("multichannel")]
    public bool? Multichannel { get; set; }

    /// <summary>
    /// Indicates whether filtering profanity was requested.
    /// </summary>
    [JsonPropertyName("profanity_filter")]
    public bool? ProfanityFilter { get; set; }

    /// <summary>
    /// Indicates whether punctuation was requested.
    /// </summary>
    [JsonPropertyName("punctuate")]
    public bool? Punctuate { get; set; }

    /// <summary>
    /// Indicates whether redaction was requested.
    /// </summary>
    // Redact is a list of strings
    [JsonPropertyName("redact")]
    public IReadOnlyList<string>? Redact { get; set; }

    /// <summary>
    /// ReadOnlyList of search terms associated with the request.
    /// </summary>
    [JsonPropertyName("search")]
    public IReadOnlyList<string>? Search { get; set; }

    /// <summary>
    /// Indicates whether SmartFormat was associated with the request.
    /// </summary>
    [JsonPropertyName("smart_format")]
    public bool? SmartFormat { get; set; }

    /// <summary>
    /// ReadOnlyList of translations associated with the request.
    /// </summary>
    [JsonPropertyName("translation")]
    public IReadOnlyList<string>? Translation { get; set; }

    /// <summary>
    /// Indicates whether utterance segmentation was requested.
    /// </summary>
    [JsonPropertyName("utterances")]
    public bool? Utterances { get; set; }
}
