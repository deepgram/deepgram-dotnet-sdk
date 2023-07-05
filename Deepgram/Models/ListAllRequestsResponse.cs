namespace Deepgram.Models;

public class ListAllRequestsResponse
{
    /// <summary>
    /// Number of results to return per page. 
    /// </summary>
    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    /// <summary>
    /// Page number that should be returned.
    /// </summary>
    [JsonPropertyName("page")]
    public int Page { get; set; }

    /// <summary>
    /// Array of requests
    /// </summary>
    [JsonPropertyName("requests")]
    public UsageRequest[] Requests { get; set; }
}
