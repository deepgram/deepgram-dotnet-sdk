﻿namespace Deepgram.Records;

public record Config
{
    [JsonPropertyName("alternatives")]
    public int? Alternatives { get; set; }

    [JsonPropertyName("callback")]
    public string? Callback { get; set; }

    [JsonPropertyName("diarize")]
    public bool? Diarize { get; set; }

    [JsonPropertyName("keywords")]
    public IReadOnlyList<string>? Keywords { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("multichannel")]
    public bool? Multichannel { get; set; }

    [JsonPropertyName("ner")]
    public bool? Ner { get; set; }

    [JsonPropertyName("numerals")]
    public bool? Numerals { get; set; }

    [JsonPropertyName("profanity_filter")]
    public bool? ProfanityFilter { get; set; }

    [JsonPropertyName("punctuate")]
    public bool? Punctuate { get; set; }

    // Redact is a list of strings
    [JsonPropertyName("redact")]
    public IReadOnlyList<string>? Redact { get; set; }

    // Search is a list of strings
    [JsonPropertyName("search")]
    public IReadOnlyList<string>? Search { get; set; }

    [JsonPropertyName("utterances")]
    public bool? Utterances { get; set; }

    public Dictionary<string, object>? AdditionalProperties { get; set; }

}