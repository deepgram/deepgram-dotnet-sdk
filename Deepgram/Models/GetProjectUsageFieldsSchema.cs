namespace Deepgram.Models;
public class GetProjectUsageFieldsSchema
{
    /// <summary>
    /// Start date of the requested date range.
    /// </summary>
    [JsonPropertyName("start")]
    public DateTime? Start { get; set; }

    /// <summary>
    /// End date of the requested date range.
    /// </summary>
    [JsonPropertyName("end")]
    public DateTime? End { get; set; }
}
