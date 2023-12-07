namespace Deepgram.Records.Live;

public record MetaData
{
    [JsonPropertyName("request_id")]
    public string RequestId { get; set; }

    [JsonPropertyName("model_info")]
    public ModelInfo ModelInfo { get; set; }

    [JsonPropertyName("model_uuid")]
    public string ModelUUId { get; set; }
}
