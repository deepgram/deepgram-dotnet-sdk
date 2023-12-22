namespace Deepgram.Records.PreRecorded;

public record Paragraph
{
    [JsonPropertyName("channel")]
    public int? Channel { get; set; }

    /// <summary>
    /// Offset in seconds from the start of the audio to where the paragraph ends.
    /// </summary>
    [JsonPropertyName("end")]
    public decimal? End { get; set; }

    /// <summary>
    /// Number of words in the paragraph
    /// </summary>
    [JsonPropertyName("num_words")]
    internal int? NumWords { get; set; }

    [JsonPropertyName("paragraphs")]
    public IReadOnlyList<Paragraph>? Paragraphs { get; set; }
    /// <summary>
    /// ReadOnly of Sentence objects.
    /// </summary>
    [JsonPropertyName("sentences")]
    public IReadOnlyList<Sentence>? Sentences { get; set; }

    [JsonPropertyName("transcript")]
    public string Transcript { get; set; }

    [JsonPropertyName("sentiments")]
    public List<Sentiments> Sentiments;

    /// <summary>
    /// Offset in seconds from the start of the audio to where the paragraph starts.
    /// </summary>
    [JsonPropertyName("start")]
    public decimal? Start { get; set; }


}

