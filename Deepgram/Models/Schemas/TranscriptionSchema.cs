namespace Deepgram.Models.Schemas;
public class TranscriptionSchema
{
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("tier")]
    public string? Tier { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("punctuate")]
    public bool? Punctuate { get; set; }

    [JsonPropertyName("profanity_filter")]
    public bool? ProfanityFilter { get; set; }

    [JsonPropertyName("redact")]
    public string[]? Redact { get; set; }

    [JsonPropertyName("diarize")]
    public bool? Diarize { get; set; }

    [JsonPropertyName("smart_format")]
    public bool? SmartFormat { get; set; }

    [JsonPropertyName("multichannel")]
    public bool? MultiChannel { get; set; }

    [JsonPropertyName("numerals")]
    public bool? Numerals { get; set; }

    [JsonPropertyName("search")]
    public string[]? Search { get; set; }

    [JsonPropertyName("replace")]
    public string[]? Replace { get; set; }

    [JsonPropertyName("callback")]
    public string? Callback { get; set; }

    [JsonPropertyName("keywords")]
    public string[]? Keywords { get; set; }

    [JsonPropertyName("tag")]
    public string[]? Tag { get; set; }

    [JsonPropertyName("additional_properties")]
    public Dictionary<string, string>? AdditionalProperties { get; }
}
