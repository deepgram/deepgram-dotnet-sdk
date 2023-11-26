namespace Deepgram.Models.Schemas;
public class GetProjectUsageRequestsSchema
{
    /// <summary>
    /// Start Date to limit search window
    /// </summary>
    [JsonPropertyName("start")]
    public string? Start { get; set; }

    /// <summary>
    /// End date to limit search window
    /// </summary>
    [JsonPropertyName("end")]
    public string? End { get; set; }

    /// <summary>
    /// limit the number of requests
    /// </summary>
    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    /// <summary>
    /// Status to search for
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }
}
