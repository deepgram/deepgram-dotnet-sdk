namespace Deepgram.Records.PreRecorded;

public record ParagraphGroup
{
    [JsonPropertyName("transcript")]
    public string Transcript { get; set; }

    [JsonPropertyName("paragraphs")]
    public IReadOnlyList<Paragraph> Paragraphs { get; set; }
}

