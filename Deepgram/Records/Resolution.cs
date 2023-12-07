namespace Deepgram.Records;

public record Resolution
{
    [JsonPropertyName("units")]
    public string? Units { get; set; }

    [JsonPropertyName("amount")]
    public int? Amount { get; set; }
}

