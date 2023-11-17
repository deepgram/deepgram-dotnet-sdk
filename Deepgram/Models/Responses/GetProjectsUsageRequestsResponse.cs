namespace Deepgram.Models.Responses;
public class GetProjectUsageRequestsResponse
{
    [JsonPropertyName("page")]
    public int? Page { get; set; }

    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    [JsonPropertyName("requests")]
    public GetProjectUsageRequestResponse[]? Requests { get; set; }
}
