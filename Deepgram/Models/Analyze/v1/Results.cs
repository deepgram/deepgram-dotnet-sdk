namespace Deepgram.Models.Analyze.v1;

public record Results
{
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
}

