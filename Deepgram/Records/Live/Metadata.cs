namespace Deepgram.Records.Live;

public record MetaData
{
    /// <summary>
    /// Unique identifier for the submitted audio and derived data returned.
    /// </summary>
    [JsonPropertyName("request_id")]
    public string? RequestId { get; set; }


    /// <summary>
    /// <see cref="ModelInfo"/>
    /// </summary>
    [JsonPropertyName("model_info")]
    public ModelInfo? ModelInfo { get; set; }

    [JsonPropertyName("model_uuid")]
    public string? ModelUUId { get; set; }
}
