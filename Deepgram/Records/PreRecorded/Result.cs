namespace Deepgram.Records.PreRecorded;

public record Result
{
    /// <summary>
    /// ReadOnlyList of <see cref="Channel"/>
    /// </summary>
    [JsonPropertyName("channels")]
    public IReadOnlyList<Channel>? Channels { get; set; }

    /// <summary>
    /// ReadOnlyList of <see cref="Utterance"/>
    /// </summary>
    [JsonPropertyName("utterances")]
    public IReadOnlyList<Utterance>? Utterances { get; set; }

    /// <summary>
    /// Transcription Summary <see cref="TranscriptionSummary"/>
    /// </summary>
    [JsonPropertyName("summary")]
    public TranscriptionSummary? Summary { get; set; }
}

