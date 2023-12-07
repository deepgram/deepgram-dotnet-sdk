namespace Deepgram.Records.PreRecorded;
public record Channel
{
    [JsonPropertyName("search")]
    public IReadOnlyList<Search> Search { get; set; }

    [JsonPropertyName("alternatives")]
    public IReadOnlyList<Alternative> Alternatives { get; set; }

    [JsonPropertyName("detected_language")]
    public string DetectedLanguage { get; set; }

    [JsonPropertyName("language_confidence")]
    public double? LanguageConfidence { get; set; }
}
