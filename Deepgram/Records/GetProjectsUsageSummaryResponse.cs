namespace Deepgram.Records;
public record GetProjectUsageSummaryResponse
{
    [JsonPropertyName("start")]
    public string? Start { get; set; }

    [JsonPropertyName("end")]
    public string? End { get; set; }

    [JsonPropertyName("resolution")]
    public Resolution? Resolution { get; set; }

    [JsonPropertyName("results")]
    public IReadOnlyList<UsageSummary>? Results { get; set; }
}
