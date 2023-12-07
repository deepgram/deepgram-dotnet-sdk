namespace Deepgram.Records;

public record UsageSummary
{
    [JsonPropertyName("start")]
    public string? Start { get; set; }

    [JsonPropertyName("end")]
    public string? End { get; set; }

    [JsonPropertyName("hours")]
    public double? Hours { get; set; }

    [JsonPropertyName("total_hours")]
    public double? TotalHours { get; set; }

    [JsonPropertyName("requests")]
    public int? Requests { get; set; }
}

