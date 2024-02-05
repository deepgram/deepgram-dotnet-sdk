namespace Deepgram.Models.PreRecorded.v1;

public record IntentsInfo
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
    public string? ModelUuid { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("output_tokens")]
    public int? OutputTokens { get; set; }
}


