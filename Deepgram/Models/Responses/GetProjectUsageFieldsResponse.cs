namespace Deepgram.Models.Responses;
public class GetProjectUsageFieldsResponse
{
    [JsonPropertyName("tags")]
    public string[]? Tags { get; set; }

    [JsonPropertyName("models")]
    public UsageModel[]? Models { get; set; }

    [JsonPropertyName("processing_methods")]
    public string[]? ProcessingMethods { get; set; }

    [JsonPropertyName("languages")]
    public string[]? Languages { get; set; }

    [JsonPropertyName("features")]
    public string[]? Features { get; set; }
}
