namespace Deepgram.Records;
public record GetProjectUsageFieldsResponse
{
    [JsonPropertyName("tags")]
    public IReadOnlyList<string>? Tags { get; set; }

    [JsonPropertyName("models")]
    public IReadOnlyList<UsageModel>? Models { get; set; }

    [JsonPropertyName("processing_methods")]
    public IReadOnlyList<string>? ProcessingMethods { get; set; }

    [JsonPropertyName("languages")]
    public IReadOnlyList<string>? Languages { get; set; }

    [JsonPropertyName("features")]
    public IReadOnlyList<string>? Features { get; set; }
}
