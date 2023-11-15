namespace Deepgram.Models;

public class Alternative
{
    [JsonPropertyName("transcript")]
    public string? Transcript { get; set; }

    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    [JsonPropertyName("words")]
    public Word[]? Words { get; set; }

    [JsonPropertyName("summaries")]
    public Summary[] Summaries { get; set; }

    [JsonPropertyName("paragraphs")]
    public ParagraphGroup[] Paragraphs { get; set; }

    [JsonPropertyName("entities")]
    public Entity Entities { get; set; }

    [JsonPropertyName("translations")]
    public Translation[] Translations { get; set; }

    [JsonPropertyName("topics")]
    public TopicGroup Topics { get; set; }
}
