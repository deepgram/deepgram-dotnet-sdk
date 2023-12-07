namespace Deepgram.Records.PreRecorded;
public record Metadata
{
    [JsonPropertyName("transaction_key")]
    public string TransactionKey { get; set; }

    [JsonPropertyName("request_id")]
    public string RequestId { get; set; }

    [JsonPropertyName("sha256")]
    public string Sha256 { get; set; }

    [JsonPropertyName("created")]
    public string Created { get; set; }

    [JsonPropertyName("duration")]
    public double Duration { get; set; }

    [JsonPropertyName("channels")]
    public int Channels { get; set; }

    [JsonPropertyName("models")]
    public string[] Models { get; set; }

    [JsonPropertyName("model_info")]
    public IReadOnlyDictionary<string, ModelInfo> ModelInfo { get; set; }

    [JsonPropertyName("warnings")]
    public IReadOnlyList<Warning>? Warnings { get; set; }
}
