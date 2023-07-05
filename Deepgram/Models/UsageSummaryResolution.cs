namespace Deepgram.Models;

public class UsageSummaryResolution
{
    /// <summary>
    /// Units of resolution amount.
    /// </summary>
    [JsonPropertyName("units")]
    public string Units { get; set; } = string.Empty;

    /// <summary>
    /// Number of days
    /// </summary>
    [JsonPropertyName("amount")]
    public int Amount { get; set; }
}
