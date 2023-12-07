namespace Deepgram.Models;

public class LiveMetadataEvent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "Metadata";

    [JsonPropertyName("transaction_key")]
    public string? TransactionKey { get; set; }

    [JsonPropertyName("request_id")]
    public string? RequestId { get; set; }

    [JsonPropertyName("sha256")]
    public string? Sha256 { get; set; }

    [JsonPropertyName("created")]
    public DateTime? Created { get; set; }

    [JsonPropertyName("duration")]
    public double? Duration { get; set; }

    [JsonPropertyName("channels")]
    public int? Channels { get; set; }
}
