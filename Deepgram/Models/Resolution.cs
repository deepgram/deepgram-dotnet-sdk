namespace Deepgram.Models;

public class Resolution
{
    [JsonPropertyName("units")]
    public string? Units { get; set; }

    [JsonPropertyName("amount")]
    public int? Amount { get; set; }
}

