namespace Deepgram.Records.PreRecorded;

public record Paragraph
{
    /// <summary>
    /// ReadOnly of Sentence objects.
    /// </summary>
    [JsonPropertyName("sentences")]
    public IReadOnlyList<Sentence>? Sentences { get; set; }

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
    /// Number of words in the paragraph
    /// </summary>
    [JsonPropertyName("num_words")]
    public int? NumWords { get; set; }
}

