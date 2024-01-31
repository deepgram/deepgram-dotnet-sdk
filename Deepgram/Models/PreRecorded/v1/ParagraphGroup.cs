namespace Deepgram.Models.PreRecorded.v1;

public record ParagraphGroup
{
    /// <summary>
    /// Full transcript
    /// </summary>
    [JsonPropertyName("transcript")]
    public string? Transcript { get; set; }

    /// <summary>
    /// ReadOnlyList of <see cref="Paragraph"/>
    /// </summary>
    [JsonPropertyName("paragraphs")]
    public IReadOnlyList<Paragraph>? Paragraphs { get; set; }
}

