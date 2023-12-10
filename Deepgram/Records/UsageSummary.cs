namespace Deepgram.Records;

public record UsageSummary
{
    /// <summary>
    /// Start date for included requests.
    /// </summary>
    [JsonPropertyName("start")]
    public DateTime StartDateTime { get; set; }

    /// <summary>
    /// End date for included requests.
    /// </summary>
    [JsonPropertyName("end")]
    public DateTime EndDateTime { get; set; }

    /// <summary>
    /// Length of time (in hours) of audio submitted in included requests.
    /// </summary>
    [JsonPropertyName("hours")]
    public decimal Hours { get; set; }

    /// <summary>
    /// Length of time (in hours) of audio processed in included requests.
    /// </summary>
    [JsonPropertyName("total_hours")]
    public decimal TotalHours { get; set; }

    /// <summary>
    /// Number of included requests.
    /// </summary>
    [JsonPropertyName("requests")]
    public int Requests { get; set; }
}

