namespace Deepgram.Records.PreRecorded;

public record Hit
{
    /// <summary>
    /// Value between 0 and 1 that indicates the model's relative confidence in this hit.
    /// </summary>
    [JsonPropertyName("confidence")]
    public decimal Confidence { get; set; }

    /// <summary>
    /// Offset in seconds from the start of the audio to where the hit occurs.
    /// </summary>
    [JsonPropertyName("start")]
    public decimal Start { get; set; }

    /// <summary>
    /// Offset in seconds from the start of the audio to where the hit ends.
    /// </summary>
    [JsonPropertyName("end")]
    public decimal End { get; set; }

    /// <summary>
    /// Transcript that corresponds to the time between start and end.
    /// </summary>
    [JsonPropertyName("snippet")]
    public string Snippet { get; set; }
}
