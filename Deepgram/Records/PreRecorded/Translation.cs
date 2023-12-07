namespace Deepgram.Records.PreRecorded;

public record Translation
{
    [JsonPropertyName("language")]
    public string Language { get; set; }

    [JsonPropertyName("translation")]
    public string Text { get; set; }
}

