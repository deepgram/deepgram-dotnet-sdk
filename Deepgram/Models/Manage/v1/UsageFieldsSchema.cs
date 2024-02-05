namespace Deepgram.Models.Manage.v1;

public class UsageFieldsSchema
{
    /// <summary>
    /// Start date of the requested date range. Format is YYYY-MM-DD. If a full timestamp is given, it will be truncated to a day. Dates are UTC. Defaults to the date of your first request.
    /// </summary>
    [JsonPropertyName("start")]
    public DateTime? Start { get; set; }

    /// <summary>
    /// End date of the requested date range. Format is YYYY-MM-DD. If a full timestamp is given, it will be truncated to a day. Dates are UTC. Defaults to the current date.
    /// </summary>
    [JsonPropertyName("end")]
    public DateTime? End { get; set; }
}
