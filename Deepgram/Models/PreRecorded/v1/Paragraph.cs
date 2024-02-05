namespace Deepgram.Models.PreRecorded.v1;

public record Paragraph
{
    /// <summary>
    /// ReadOnly of Sentence objects.
    /// </summary>
    [JsonPropertyName("sentences")]
    public IReadOnlyList<Sentence>? Sentences { get; set; }

    /// <summary>
    /// Number of words in the paragraph
    /// </summary>
    [JsonPropertyName("num_words")]
    internal int? NumWords { get; set; }

    /// <summary>
    /// Offset in seconds from the start of the audio to where the paragraph starts.
    /// </summary>
    [JsonPropertyName("start")]
    public decimal? Start { get; set; }

    /// <summary>
    /// Offset in seconds from the start of the audio to where the paragraph ends.
    /// </summary>
    [JsonPropertyName("end")]
    public decimal? End { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("sentiment")]
    public string? Sentiment { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("sentiment_score")]
    public double? SentimentScore { get; set; }
}

