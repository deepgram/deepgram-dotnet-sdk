namespace Deepgram.Models.Manage.v1;
public record UsageRequestsResponse
{
    [JsonPropertyName("page")]
    public int? Page { get; set; }

    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    [JsonPropertyName("requests")]
    public IReadOnlyList<UsageRequestResponse>? Requests { get; set; }
}
