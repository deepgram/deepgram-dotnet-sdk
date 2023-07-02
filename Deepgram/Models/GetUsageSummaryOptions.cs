namespace Deepgram.Models;

public class GetUsageSummaryOptions
{
    /// <summary>
    /// Start date of the requested date range.
    /// </summary>
    [JsonProperty("start")]
    public DateTime? StartDateTime { get; set; }

    /// <summary>
    /// End date of the requested date range.
    /// </summary>
    [JsonProperty("end")]
    public DateTime? EndDateTime { get; set; }

    /// <summary>
    /// Limits results to requests made using the API key corresponding to the given accessor. 
    /// </summary>
    [JsonProperty("accessor")]
    public string ApiKeyId { get; set; }

    /// <summary>
    /// Limits results to requests associated with the specified tag(s). 
    /// </summary>
    [JsonProperty("tag")]
    public string[]? Tag { get; set; }

    /// <summary>
    /// Limits results to requests processed using the specified method.
    /// </summary>
    [JsonProperty("method")]
    public RequestMethod? Method { get; set; }

    /// <summary>
    /// Limits results to requests run with the specified model applied.
    /// </summary>
    [JsonProperty("model")]
    public string[]? Model { get; set; }

    /// <summary>
    /// Limits results to requests that include the multichannel feature.
    /// </summary>
    [JsonProperty("multichannel")]
    public bool? MultiChannel { get; set; }

    /// <summary>
    /// Limits results to requests that include the interim_results feature.
    /// </summary>
    [JsonProperty("interim_results")]
    public bool? InterimResults { get; set; }

    /// <summary>
    /// Limits results to requests that include the punctuate feature.
    /// </summary>
    [JsonProperty("punctuate")]
    public bool? Punctuate { get; set; }

    /// <summary>
    /// Limits results to requests that include the ner feature.
    /// </summary>
    [JsonProperty("ner")]
    public bool? NamedEntityRecognition { get; set; }

    /// <summary>
    /// Limits results to requests that include the utterances feature.
    /// </summary>
    [JsonProperty("utterances")]
    public bool? Utterances { get; set; }

    /// <summary>
    /// Limits results to requests that include the replace feature.
    /// </summary>
    [JsonProperty("replace")]
    public bool? Replace { get; set; }

    /// <summary>
    /// Limits results to requests that include the profanity_filter feature.
    /// </summary>
    [JsonProperty("profanity_filter")]
    public bool? ProfanityFilter { get; set; }

    /// <summary>
    /// Limits results to requests that include the keywords feature.
    /// </summary>
    [JsonProperty("keywords")]
    public bool? Keywords { get; set; }

    /// <summary>
    /// Limits results to requests that include the sentiment feature.
    /// </summary>
    [JsonProperty("analyze_sentiment")]
    public bool? Sentiment { get; set; }

    /// <summary>
    /// Limits results to requests that include the sentiment threshold feature.
    /// </summary>
    [JsonProperty("sentiment_threshold")]
    public bool? SentimentThreshold { get; set; }

    /// <summary>
    /// Limits results to requests that include the diarize feature.
    /// </summary>
    [JsonProperty("diarize")]
    public bool? Diarization { get; set; }

    /// <summary>
    /// Limits results to requests that include the detect_language feature.
    /// </summary>
    [JsonProperty("detect_language")]
    public bool? DetectLanguage { get; set; }

    /// <summary>
    /// Limits results to requests that include the search feature.
    /// </summary>
    [JsonProperty("search")]
    public bool? Search { get; set; }

    /// <summary>
    /// Limits results to requests that include the redact feature.
    /// </summary>
    [JsonProperty("redact")]
    public bool? Redaction { get; set; }

    /// <summary>
    /// Limits results to requests that include the alternatives feature.
    /// </summary>
    [JsonProperty("alternatives")]
    public bool? Alternatives { get; set; }

    /// <summary>
    /// Limits results to requests that include the numerals feature.
    /// </summary>
    [JsonProperty("numerals")]
    public bool? Numerals { get; set; }

    /// <summary>
    /// Limits results to requests that include the paragraphs feature.
    /// </summary>
    [JsonProperty("paragraphs")]
    public bool? Paragraphs { get; set; }

    /// <summary>
    /// Limits results to requests that include the detect entities feature.
    /// </summary>
    [JsonProperty("detect_entities")]
    public bool? DetectEntities { get; set; }

    /// <summary>
    /// Limits results to requests that include the summarize feature.
    /// </summary>
    [JsonProperty("summarize")]
    public bool? Summarize { get; set; }

    /// <summary>
    /// Limits results to requests that include the translation feature.
    /// </summary>
    [JsonProperty("translate")]
    public bool? Translate { get; set; }

    /// <summary>
    /// Limits results to requests that include the topic detection feature.
    /// </summary>
    [JsonProperty("detect_topics")]
    public bool? DetectTopics { get; set; }
}
