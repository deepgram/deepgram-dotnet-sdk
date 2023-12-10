namespace Deepgram.Records;

public record Config
{
    /// <summary>
    /// Requested maximum number of transcript alternatives to return.
    /// </summary>
    [JsonPropertyName("alternatives")]
    public int? Alternatives { get; set; }

    [JsonPropertyName("callback")]
    public string? Callback { get; set; }

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
    /// Indicates whether named-entity recognition (NER) was requested.
    /// </summary>
    [JsonPropertyName("ner")]
    public bool? Ner { get; set; }

    /// <summary>
    /// Indicates whether numeral conversion was requested.
    /// </summary>
    [JsonPropertyName("numerals")]
    public bool? Numerals { get; set; }

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
    /// Indicates whether utterance segmentation was requested.
    /// </summary>
    [JsonPropertyName("utterances")]
    public bool? Utterances { get; set; }

    /// <summary>
    /// ReadOnlyList of translations associated with the request.
    /// </summary>
    [JsonPropertyName("translation")]
    public IReadOnlyList<string>? Translation { get; set; }

    /// <summary>
    /// Indicates whether topic detection was requested.
    /// </summary>
    [JsonPropertyName("detect_topics")]
    public bool? DetectTopics { get; set; }

}
