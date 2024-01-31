namespace Deepgram.Models.PreRecorded.v1;

public record TranscriptionSummary
{
    [JsonPropertyName("result")]
    public string? Result { get; set; }

    [JsonPropertyName("short")]
    public string? Short { get; set; }
}

