namespace Deepgram.Records.PreRecorded;
public record Channel
{
    /// <summary>
    /// ReadOnlyList of Search objects.
    /// </summary>
    [JsonPropertyName("search")]
    public IReadOnlyList<Search>? Search { get; set; }

    /// <summary>
    /// ReadOnlyList of <see cref="Alternative"/> objects.
    /// </summary>
    [JsonPropertyName("alternatives")]
    public IReadOnlyList<Alternative>? Alternatives { get; set; }

    /// <summary>
    /// BCP-47 language tag for the dominant language identified in the channel.
    /// </summary>
    /// <remark>Only available in pre-recorded requests</remark>
    [JsonPropertyName("detected_language")]
    public string? DetectedLanguage { get; set; }


    [JsonPropertyName("language_confidence")]
    public double? LanguageConfidence { get; set; }
}
