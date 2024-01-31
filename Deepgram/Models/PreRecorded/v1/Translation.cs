namespace Deepgram.Models.PreRecorded.v1;

public record Translation
{
    /// <summary>
    /// Translated transcript.
    /// </summary>
    [JsonPropertyName("translation")]
    public string? TranslatedTranscript { get; set; }

    /// <summary>
    /// Language code of the translation.
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }
}

