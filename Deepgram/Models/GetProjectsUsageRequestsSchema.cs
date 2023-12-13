namespace Deepgram.Models;
public class GetProjectUsageRequestsSchema
{
    // <summary>
    /// Start date of the requested date range. Formats accepted are YYYY-MM-DD, YYYY-MM-DDTHH:MM:SS, or YYYY-MM-DDTHH:MM:SS+HH:MM.
    /// </summary>
    [JsonPropertyName("start")]
    public DateTime? Start { get; set; }

    /// <summary>
    /// End date of the requested date range. Formats accepted are YYYY-MM-DD, YYYY-MM-DDTHH:MM:SS, or YYYY-MM-DDTHH:MM:SS+HH:MM.
    /// </summary>
    [JsonPropertyName("end")]
    public DateTime? End { get; set; }

    /// <summary>
    /// Number of results to return per page. Default 10. Range [1,100].
    /// </summary>

    [JsonPropertyName("limit")]
    public int Limit { get; set; } = 10;

    /// <summary>
    /// Status of requests to return. Enables you to filter requests depending on whether they have succeeded or failed. If not specified, returns requests with all statuses.
    /// </summary>
    /// <remarks>Possible Values: null, succeeded OR failed</remarks>
    [JsonPropertyName("status")]
    public string? Status { get; set; }
}
