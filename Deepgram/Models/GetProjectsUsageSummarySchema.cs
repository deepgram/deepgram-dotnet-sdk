namespace Deepgram.Models;
public class GetProjectsUsageSummarySchema
{
    /// <summary>
    /// Name of who made request
    /// </summary>
    [JsonPropertyName("accessor")]
    public string? Accessor { get; set; }

    /// <summary>
    /// Limits results to requests that include the alternatives feature.
    /// </summary>
    [JsonPropertyName("alternatives")]
    public bool? Alternatives { get; set; }

    /// <summary>
    /// Start date of the requested date range.
    /// </summary>
    [JsonPropertyName("start")]
    public DateTime? Start { get; set; }

    /// <summary>
    /// End date of the requested date range.
    /// </summary>
    [JsonPropertyName("end")]
    public DateTime? End { get; set; }

    /// <summary>
    /// Limits results to requests associated with the specified tag(s). 
    /// </summary>
    [JsonPropertyName("tag")]
    public List<string>? Tag { get; set; }

    /// <summary>
    /// Permitted values "sync" | "async" | "streaming"
    /// <see cref="RequestMethod">
    /// </summary>
    [JsonPropertyName("method")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RequestMethod? Method { get; set; }

    /// <summary>
    /// Limits results to requests run with the specified model applied.
    /// </summary>
    [JsonPropertyName("model")]
    public List<string> Model { get; set; }

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
    public bool? Ner { get; set; }

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
    /// Limits results to requests that include the search feature.
    /// </summary>
    [JsonPropertyName("search")]
    public bool? Search { get; set; }

    /// <summary>
    /// Limits results to requests that include the redact feature.
    /// </summary>
    [JsonPropertyName("redact")]
    public bool? Redact { get; set; }

    /// <summary>
    /// Limits results to requests that include the numerals feature.
    /// </summary>
    [JsonPropertyName("numerals")]
    public bool? Numerals { get; set; }

    [JsonPropertyName("smart_format")]
    public bool? SmartFormat { get; set; }
}
