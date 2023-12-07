namespace Deepgram.Records.PreRecorded;

public record Alternative
{
    [JsonPropertyName("transcript")]
    public string Transcript { get; set; }

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    [JsonPropertyName("words")]
    public IReadOnlyList<WordBase> Words { get; set; }

    [JsonPropertyName("summaries")]
    public IReadOnlyList<Summary> Summaries { get; set; }

    [JsonPropertyName("paragraphs")]
    public IReadOnlyList<ParagraphGroup> Paragraphs { get; set; }

    [JsonPropertyName("entities")]
    public Entity Entities { get; set; }

    [JsonPropertyName("translations")]
    public IReadOnlyList<Translation> Translations { get; set; }

    [JsonPropertyName("topics")]
    public TopicGroup Topics { get; set; }
}
