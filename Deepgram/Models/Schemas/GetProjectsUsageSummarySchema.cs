namespace Deepgram.Models.Schemas;
public class GetProjectUsageSummarySchema
{
    [JsonPropertyName("start")]
    public string? Start { get; set; }

    [JsonPropertyName("end")]
    public string? End { get; set; }

    [JsonPropertyName("accessor")]
    public string? Accessor { get; set; }

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