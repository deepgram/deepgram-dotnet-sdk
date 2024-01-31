namespace Deepgram.Models.PreRecorded.v1;

public record Entity
{
    /// <summary>
    /// This is the type of the entity
    /// </summary>
    /// <remarks>e.g. DATE, PER, ORG, etc.</remarks>
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// This is the value of the detected entity.
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    /// <summary>
    /// Starting index of the entities words within the transcript.
    /// </summary>
    [JsonPropertyName("start_word")]
    public int? StartWord { get; set; }

    /// <summary>
    /// Ending index of the entities words within the transcript.
    /// </summary>
    [JsonPropertyName("end_word")]
    public int? EndWord { get; set; }

    /// <summary>
    /// Value between 0 and 1 indicating the model's relative confidence in this detected entity.
    /// </summary>
    [JsonPropertyName("confidence")]
    public decimal? Confidence { get; set; }
}
