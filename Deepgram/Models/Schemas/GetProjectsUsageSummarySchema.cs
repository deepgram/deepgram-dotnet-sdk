namespace Deepgram.Models.Schemas;
public class GetProjectsUsageSummarySchema
{
    /// <summary>
    /// Start date to limit range of requests to summarize
    /// </summary>
    [JsonPropertyName("start")]
    public DateTime? Start { get; set; }

    /// <summary>
    /// End date to limit range of requests to summarize
    /// </summary>
    [JsonPropertyName("end")]
    public DateTime? End { get; set; }

    /// <summary>
    /// Name of who made request
    /// </summary>
    [JsonPropertyName("accessor")]
    public string? Accessor { get; set; }

    /// <summary>
    /// tags associated with request
    /// </summary>
    [JsonPropertyName("tag")]
    public string? Tag { get; set; }


    [JsonPropertyName("method")]
    public string? Method { get; set; }


    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("multichannel")]
    public bool? MultiChannel { get; set; }


    [JsonPropertyName("interim_results")]
    public bool? InterimResults { get; set; }

    [JsonPropertyName("punctuate")]
    public bool? Punctuate { get; set; }

    [JsonPropertyName("ner")]
    public bool? Ner { get; set; }

    [JsonPropertyName("utterances")]
    public bool? Utterances { get; set; }

    [JsonPropertyName("replace")]
    public bool? Replace { get; set; }

    [JsonPropertyName("profanity_filter")]
    public bool? ProfanityFilter { get; set; }

    [JsonPropertyName("keywords")]
    public bool? Keywords { get; set; }

    [JsonPropertyName("detect_topics")]
    public bool? DetectTopics { get; set; }

    [JsonPropertyName("diarize")]
    public bool? Diarize { get; set; }

    [JsonPropertyName("search")]
    public bool? Search { get; set; }

    [JsonPropertyName("redact")]
    public bool? Redact { get; set; }

    [JsonPropertyName("alternatives")]
    public bool? Alternatives { get; set; }

    [JsonPropertyName("numerals")]
    public bool? Numerals { get; set; }

    [JsonPropertyName("smart_format")]
    public bool? SmartFormat { get; set; }
}
