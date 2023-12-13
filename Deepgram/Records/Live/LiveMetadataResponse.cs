namespace Deepgram.Records.Live;

public record LiveMetadataResponse
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LiveType Type { get; set; } = LiveType.Metadata;

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

    [JsonPropertyName("models")]
    public IReadOnlyList<string> Models { get; set; }
}
