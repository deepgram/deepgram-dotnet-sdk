namespace Deepgram.Models.PreRecorded.v1;

public record Results
{
    /// <summary>
    /// ReadOnlyList of <see cref="Channel"/>
    /// </summary>
    [JsonPropertyName("channels")]
    public IReadOnlyList<Channel>? Channels { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("intents")]
    public IntentGroup? Intents { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("sentiments")]
    public SentimentGroup? Sentiments { get; set; }

    /// <summary>
    /// Transcription Summary <see cref="TranscriptionSummary"/>
    /// </summary>
    [JsonPropertyName("summary")]
    public Summary? Summary { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("topics")]
    public TopicGroup? Topics { get; set; }

    /// <summary>
    /// ReadOnlyList of <see cref="Utterance"/>
    /// </summary>
    [JsonPropertyName("utterances")]
    public IReadOnlyList<Utterance>? Utterances { get; set; }
}

