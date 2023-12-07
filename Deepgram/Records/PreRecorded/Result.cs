namespace Deepgram.Records.PreRecorded;

public record Result
{
    [JsonPropertyName("channels")]
    public IReadOnlyList<Channel> Channels { get; set; }

    [JsonPropertyName("utterances")]
    public IReadOnlyList<Utterance> Utterances { get; set; }

    [JsonPropertyName("summary")]
    public TranscriptionSummary Summary { get; set; }
}

