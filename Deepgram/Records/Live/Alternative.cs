namespace Deepgram.Records.Live;

public record Alternative
{
    /// <summary>
    /// Single-string transcript containing what the model hears in this channel of audio.
    /// </summary>
    [JsonPropertyName("transcript")]
    public string? Transcript { get; set; }
    /// <summary>
    /// Value between 0 and 1 indicating the model's relative confidence in this transcript.
    /// </summary>
    [JsonPropertyName("confidence")]
    public decimal? Confidence { get; set; }

    /// <summary>
    /// ReadOnly List of <see cref="Word"/> objects.
    /// </summary>
    [JsonPropertyName("words")]
    public IReadOnlyList<Word>? Words { get; set; }


}
