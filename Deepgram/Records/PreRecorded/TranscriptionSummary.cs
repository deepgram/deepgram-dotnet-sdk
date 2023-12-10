namespace Deepgram.Records.PreRecorded;

public record TranscriptionSummary
{
    [JsonPropertyName("result")]
    public string? Result { get; set; }

    [JsonPropertyName("short")]
    public string? Short { get; set; }
}

