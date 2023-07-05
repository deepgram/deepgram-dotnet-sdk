namespace Deepgram.Models;

public class ParagraphGroup
{
    /// <summary>
    /// Full transcript
    /// </summary>
    [JsonPropertyName("transcript")]
    public string Transcript { get; set; } = string.Empty;

    /// <summary>
    /// Array of Paragraph objects.
    /// </summary>
    [JsonPropertyName("paragraphs")]
    public Paragraph[] Paragraphs { get; set; }
}
