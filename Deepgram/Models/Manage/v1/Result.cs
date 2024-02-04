namespace Deepgram.Models.Manage.v1;

public record UsageSummary
{
    /// <summary>
    /// Start date for included requests.
    /// </summary>
    [JsonPropertyName("start")]
    public DateTime? StartDateTime { get; set; }

    /// <summary>
    /// End date for included requests.
    /// </summary>
    [JsonPropertyName("end")]
    public DateTime? EndDateTime { get; set; }

    /// <summary>
    /// Length of time (in hours) of audio submitted in included requests.
    /// </summary>
    [JsonPropertyName("hours")]
    public double? Hours { get; set; }

    /// <summary>
    /// Length of time (in hours) of audio processed in included requests.
    /// </summary>
    [JsonPropertyName("total_hours")]
    public double? TotalHours { get; set; }

    /// <summary>
    /// Number of included requests.
    /// </summary>
    [JsonPropertyName("requests")]
    public int? Requests { get; set; }

    /// <summary>
    /// Token information
    /// </summary>
    [JsonPropertyName("tokens")]
    public Token? Tokens { get; set; }
}

