namespace Deepgram.Models;

public class GetUsageSummaryOptions
{
    /// <summary>
    /// Start date of the requested date range.
    /// </summary>
    [JsonPropertyName("start")]
    public DateTime? StartDateTime { get; set; }

    /// <summary>
    /// End date of the requested date range.
    /// </summary>
    [JsonPropertyName("end")]
    public DateTime? EndDateTime { get; set; }

    /// <summary>
    /// Limits results to requests made using the API key corresponding to the given accessor. 
    /// </summary>
    [JsonPropertyName("accessor")]
    public string ApiKeyId { get; set; }

    /// <summary>
    /// Limits results to requests associated with the specified tag(s). 
    /// </summary>
    [JsonPropertyName("tag")]
    public string[]? Tag { get; set; }

    /// <summary>
    /// Limits results to requests processed using the specified method.
    /// </summary>
    [JsonPropertyName("method")]
    public RequestMethod? Method { get; set; }

    /// <summary>
    /// Limits results to requests run with the specified model applied.
    /// </summary>
    [JsonPropertyName("model")]
    public string[]? Model { get; set; }

    /// <summary>
    /// Limits results to requests that include the multichannel feature.
    /// </summary>
    [JsonPropertyName("multichannel")]
    public bool? MultiChannel { get; set; }

    /// <summary>
    /// Limits results to requests that include the interim_results feature.
    /// </summary>
    [JsonPropertyName("interim_results")]
    public bool? InterimResults { get; set; }

    /// <summary>
    /// Limits results to requests that include the punctuate feature.
    /// </summary>
    [JsonPropertyName("punctuate")]
    public bool? Punctuate { get; set; }

    /// <summary>
    /// Limits results to requests that include the ner feature.
    /// </summary>
    [JsonPropertyName("ner")]
    public bool? NamedEntityRecognition { get; set; }

    /// <summary>
    /// Limits results to requests that include the utterances feature.
    /// </summary>
    [JsonPropertyName("utterances")]
    public bool? Utterances { get; set; }

    /// <summary>
    /// Limits results to requests that include the replace feature.
    /// </summary>
    [JsonPropertyName("replace")]
    public bool? Replace { get; set; }

    /// <summary>
    /// Limits results to requests that include the profanity_filter feature.
    /// </summary>
    [JsonPropertyName("profanity_filter")]
    public bool? ProfanityFilter { get; set; }

    /// <summary>
    /// Limits results to requests that include the keywords feature.
    /// </summary>
    [JsonPropertyName("keywords")]
    public bool? Keywords { get; set; }

    /// <summary>
    /// Limits results to requests that include the sentiment feature.
    /// </summary>
    [JsonPropertyName("analyze_sentiment")]
    public bool? Sentiment { get; set; }

    /// <summary>
    /// Limits results to requests that include the sentiment threshold feature.
    /// </summary>
    [JsonPropertyName("sentiment_threshold")]
    public bool? SentimentThreshold { get; set; }

    /// <summary>
    /// Limits results to requests that include the diarize feature.
    /// </summary>
    [JsonPropertyName("diarize")]
    public bool? Diarization { get; set; }

    /// <summary>
    /// Limits results to requests that include the detect_language feature.
    /// </summary>
    [JsonPropertyName("detect_language")]
    public bool? DetectLanguage { get; set; }

    /// <summary>
    /// Limits results to requests that include the search feature.
    /// </summary>
    [JsonPropertyName("search")]
    public bool? Search { get; set; }

    /// <summary>
    /// Limits results to requests that include the redact feature.
    /// </summary>
    [JsonPropertyName("redact")]
    public bool? Redaction { get; set; }

    /// <summary>
    /// Limits results to requests that include the alternatives feature.
    /// </summary>
    [JsonPropertyName("alternatives")]
    public bool? Alternatives { get; set; }

    /// <summary>
    /// Limits results to requests that include the numerals feature.
    /// </summary>
    [JsonPropertyName("numerals")]
    public bool? Numerals { get; set; }

    /// <summary>
    /// Limits results to requests that include the paragraphs feature.
    /// </summary>
    [JsonPropertyName("paragraphs")]
    public bool? Paragraphs { get; set; }

    /// <summary>
    /// Limits results to requests that include the detect entities feature.
    /// </summary>
    [JsonPropertyName("detect_entities")]
    public bool? DetectEntities { get; set; }

    /// <summary>
    /// Limits results to requests that include the summarize feature.
    /// </summary>
    [JsonPropertyName("summarize")]
    public bool? Summarize { get; set; }

    /// <summary>
    /// Limits results to requests that include the translation feature.
    /// </summary>
    [JsonPropertyName("translate")]
    public bool? Translate { get; set; }

    /// <summary>
    /// Limits results to requests that include the topic detection feature.
    /// </summary>
    [JsonPropertyName("detect_topics")]
    public bool? DetectTopics { get; set; }
}
