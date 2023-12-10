namespace Deepgram.Records.PreRecorded;

public record TopicGroup
{
    /// <summary>
    /// ReadonlyList of <see cref="Topic"/>
    /// </summary>
    [JsonPropertyName("topics")]
    public IReadOnlyList<Topic>? Topics { get; set; }

    /// <summary>
    /// Transcript covered by the topic.
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; set; }

    /// <summary>
    /// Word position in transcript where the topic begins
    /// </summary>
    [JsonPropertyName("start_word")]
    public int StartWord { get; set; }

    /// <summary>
    /// Word position in transcript where the topic ends
    /// </summary>
    [JsonPropertyName("end_word")]
    public int EndWord { get; set; }
}

