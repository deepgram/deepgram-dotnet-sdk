namespace Deepgram.Models;

public class GetUsageFieldsOptions
{
    /// <summary>
    /// Start date of the requested date range.
    /// </summary>
    [JsonPropertyName("start")]
    public DateTime? StartDateTime { get; set; }

    /// <summary>
    /// End date of the requested date range.
    /// </summary>
    [JsonPropertyName("end")]
    public DateTime? EndDateTime { get; set; }
}
