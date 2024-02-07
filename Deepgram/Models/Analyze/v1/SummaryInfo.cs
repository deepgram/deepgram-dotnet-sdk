namespace Deepgram.Models.Analyze.v1;

public record SummaryInfo
{
    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("input_tokens")]
    public int? InputTokens { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("model_uuid")]
    public string? ModelUUID { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("output_tokens")]
    public int? OutputTokens { get; set; }
}
