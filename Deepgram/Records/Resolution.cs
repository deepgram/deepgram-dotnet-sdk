namespace Deepgram.Records;

public record Resolution
{
    /// <summary>
    /// Units of resolution amount.
    /// </summary>
    [JsonPropertyName("units")]
    public string? Units { get; set; }

    /// <summary>
    /// Number of days
    /// </summary>
    [JsonPropertyName("amount")]
    public int? Amount { get; set; }
}

