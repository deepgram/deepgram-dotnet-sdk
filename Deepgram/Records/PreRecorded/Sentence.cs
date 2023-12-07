namespace Deepgram.Records.PreRecorded;

public record Sentence
{
    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("start")]
    public int Start { get; set; }

    [JsonPropertyName("end")]
    public int End { get; set; }
}

