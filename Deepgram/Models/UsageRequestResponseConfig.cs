namespace Deepgram.Models;

public class UsageRequestResponseConfig
{
    /// <summary>
    /// Requested maximum number of transcript alternatives to return.
    /// </summary>
    [JsonPropertyName("alternatives")]
    public int? Alternatives { get; set; }

    /// <summary>
    /// Indicates whether diarization was requested.
    /// </summary>
    [JsonPropertyName("diarize")]
    public bool? Diarize { get; set; }

    /// <summary>
    /// Indicates whether multichannel processing was requested.
    /// </summary>
    [JsonPropertyName("multichannel")]
    public bool? MultiChannel { get; set; }

    /// <summary>
    /// Indicates whether detect language feature was requested.
    /// </summary>
    [JsonPropertyName("detect_language")]
    public bool? DetectLanguage { get; set; }

    /// <summary>
    /// Indicates whether paragraphs feature was requested.
    /// </summary>
    [JsonPropertyName("paragraphs")]
    public bool? Paragraphs { get; set; }

    /// <summary>
    /// Indicates whether detect entities feature was requested.
    /// </summary>
    [JsonPropertyName("detect_entities")]
    public bool? DetectEntities { get; set; }

    /// <summary>
    /// Indicates whether summarize feature was requested.
    /// </summary>
    [JsonPropertyName("summarize")]
    public bool? Summarize { get; set; }

    /// <summary>
    /// Array of keywords associated with the request.
    /// </summary>
    [JsonPropertyName("keywords")]
    public string[]? Keywords { get; set; }

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
    /// Indicates whether named-entity recognition (NER) was requested.
    /// </summary>
    [JsonPropertyName("ner")]
    public bool? NamedEntityRecognition { get; set; }

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
    [JsonPropertyName("redact")]
    public string[]? Redaction { get; set; }

    /// <summary>
    /// Array of search terms associated with the request.
    /// </summary>
    [JsonPropertyName("search")]
    public string[]? Search { get; set; }

    /// <summary>
    /// Indicates whether utterance segmentation was requested.
    /// </summary>
    [JsonPropertyName("utterances")]
    public bool? Utterances { get; set; }

    /// <summary>
    /// Array of translations associated with the request.
    /// </summary>
    [JsonPropertyName("translation")]
    public string[]? Translation { get; set; }

    /// <summary>
    /// Indicates whether topic detection was requested.
    /// </summary>
    [JsonPropertyName("detect_topics")]
    public bool? DetectTopics { get; set; }
}
