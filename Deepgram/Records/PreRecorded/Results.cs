namespace Deepgram.Records.PreRecorded;

public record Results
{
    /// <summary>
    /// ReadOnlyList of <see cref="Channel"/>
    /// </summary>
    [JsonPropertyName("channels")]
    public IReadOnlyList<Channel>? Channels { get; set; }

    [JsonPropertyName("intents")]
    public Intents Intents { get; set; }


    [JsonPropertyName("paragraphs")]
    public ParagraphGroup? ParagraphGroup { get; set; }

    /// <summary>
    /// Transcription Summary <see cref="TranscriptionSummary"/>
    /// </summary>
    [JsonPropertyName("summary")]
    public TranscriptionSummary? Summary { get; set; }

    /// <summary>
    /// ReadOnlyList of <see cref="Utterance"/>
    /// </summary>
    [JsonPropertyName("utterances")]
    public IReadOnlyList<Utterance>? Utterances { get; set; }

    [JsonPropertyName("sentiments")]
    public Sentiments Sentiments { get; set; }
}

