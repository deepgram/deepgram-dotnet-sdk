namespace Deepgram.Models.Manage.v1;
public record UsageRequestsResponse
{
    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("page")]
    public int? Page { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("requests")]
    public IReadOnlyList<UsageRequestResponse>? Requests { get; set; }
}
