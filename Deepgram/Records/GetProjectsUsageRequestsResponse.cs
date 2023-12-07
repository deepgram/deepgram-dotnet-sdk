namespace Deepgram.Records;
public record GetProjectUsageRequestsResponse
{
    [JsonPropertyName("page")]
    public int? Page { get; set; }

    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    [JsonPropertyName("requests")]
    public IReadOnlyList<GetProjectUsageRequestResponse>? Requests { get; set; }
}
