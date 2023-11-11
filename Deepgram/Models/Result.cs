namespace Deepgram.Models;

public class Result
{
    [JsonPropertyName("channels")]
    public Channel[]? Channels { get; set; }

    [JsonPropertyName("utterances")]
    public Utterance[]? Utterances { get; set; }

    [JsonPropertyName("summary")]
    public TranscriptionSummary? Summary { get; set; }
}

