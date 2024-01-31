namespace Deepgram.Models.Manage.v1;
public record GetProjectUsageSummaryResponse
{
    /// <summary>
    /// Start date for included requests.
    /// </summary>
    [JsonPropertyName("start")]
    public DateTime? Start { get; set; }

    /// <summary>
    /// End date for included requests.
    /// </summary>
    [JsonPropertyName("end")]
    public DateTime? End { get; set; }

    /// <summary>
    /// Resolution of the usage <see cref="v1.Resolution"/>
    /// </summary>
    [JsonPropertyName("resolution")]
    public Resolution? Resolution { get; set; }

    /// <summary>
    /// Result summaries <see cref="UsageSummary"/>
    /// </summary>
    [JsonPropertyName("results")]
    public IReadOnlyList<UsageSummary>? Results { get; set; }
}
