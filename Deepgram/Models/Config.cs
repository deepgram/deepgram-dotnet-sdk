namespace Deepgram.Models;

public class Config
{
    [JsonPropertyName("alternatives")]
    public int? Alternatives { get; set; }

    [JsonPropertyName("callback")]
    public string? Callback { get; set; }

    [JsonPropertyName("diarize")]
    public bool? Diarize { get; set; }

    [JsonPropertyName("keywords")]
    public string[]? Keywords { get; set; }

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
    public string[]? Redact { get; set; }

    // Search is a list of strings
    [JsonPropertyName("search")]
    public string? Search { get; set; }

}
