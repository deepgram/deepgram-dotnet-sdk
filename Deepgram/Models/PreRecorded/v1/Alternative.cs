namespace Deepgram.Models.PreRecorded.v1;

public record Alternative
{

    /// <summary>
    /// Value between 0 and 1 indicating the model's relative confidence in this transcript.
    /// </summary>
    [JsonPropertyName("confidence")]
    public decimal? Confidence { get; set; }

    /// <summary>
    /// ReadOnlyList of <see cref="Entity"/> objects.
    /// </summary>
    /// <remark>Only used when the detect entities feature is enabled on the request</remark>
    [JsonPropertyName("entities")]
    public IReadOnlyList<Entity>? Entities { get; set; }

    /// <summary>
    /// ReadOnly List of <see cref="ParagraphGroup"/> containing  separated transcript and <see cref="Paragraph"/> objects.
    /// </summary>
    /// <remark>Only used when the paragraph feature is enabled on the request</remark>
    [JsonPropertyName("paragraphs")]
    public ParagraphGroup? Paragraphs { get; set; }
    /// <summary>
    /// ReadOnly List of <see cref="Summary "/> objects.
    /// </summary>
    /// <remark>Only used when the summarize feature is enabled on the request</remark>
    [JsonPropertyName("summaries")]
    public IReadOnlyList<Summary>? Summaries { get; set; }

    /// <summary>
    /// Single-string transcript containing what the model hears in this channel of audio.
    /// </summary>
    [JsonPropertyName("transcript")]
    public string? Transcript { get; set; }


    /// <summary>
    /// ReadOnlyList of <see cref="Translation"/> objects.
    /// </summary>
    /// <remark>Only used when the translation feature is enabled on the request</remark>
    [JsonPropertyName("translations")]
    public IReadOnlyList<Translation>? Translations { get; set; }

    /// <summary>
    /// Group of Topics<see cref="TopicGroup"/>
    /// </summary>
    [JsonPropertyName("topics")]
    public IReadOnlyList<TopicGroup>? Topics { get; set; }

    /// <summary>
    /// ReadOnly List of <see cref="Word"/> objects.
    /// </summary>
    [JsonPropertyName("words")]
    public IReadOnlyList<Word>? Words { get; set; }

    [JsonPropertyName("sentiment_segments")]
    public List<SentimentSegment>? SentimentSegments { get; set; }
}
