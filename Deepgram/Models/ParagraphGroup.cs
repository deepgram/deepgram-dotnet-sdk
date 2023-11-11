namespace Deepgram.Models;

public class ParagraphGroup
{
    [JsonPropertyName("transcript")]
    public string? Transcript { get; set; }

    [JsonPropertyName("paragraphs")]
    public Paragraph[]? Paragraphs { get; set; }
}

