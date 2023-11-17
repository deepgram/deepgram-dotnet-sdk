namespace Deepgram.Models.Schemas;
public class GetProjectUsageRequestsSchema
{
    [JsonPropertyName("start")]
    public string? Start { get; set; }

    [JsonPropertyName("end")]
    public string? End { get; set; }

    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}