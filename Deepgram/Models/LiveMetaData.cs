namespace Deepgram.Models;

public class LiveMetaData
{
    [JsonPropertyName("request_id")]
    public string? RequestId { get; set; }

    [JsonPropertyName("model_info")]
    public ModelInfo? ModelInfo { get; set; }

    [JsonPropertyName("model_uuid")]
    public string? ModelUUId { get; set; }
}
