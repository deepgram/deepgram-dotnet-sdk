namespace Deepgram.Models.Responses;
public class GetProjectUsageSummaryResponse
{
    [JsonPropertyName("start")]
    public string? Start { get; set; }

    [JsonPropertyName("end")]
    public string? End { get; set; }

    [JsonPropertyName("resolution")]
    public Resolution? Resolution { get; set; }

    [JsonPropertyName("results")]
    public UsageSummary[]? Results { get; set; }
}
