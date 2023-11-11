namespace Deepgram.Models;

public class TranscriptionSummary
{
    [JsonPropertyName("result")]
    public string? Result { get; set; }

    [JsonPropertyName("short")]
    public string? Short { get; set; }
}

