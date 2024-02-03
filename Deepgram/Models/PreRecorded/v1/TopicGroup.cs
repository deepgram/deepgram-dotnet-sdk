namespace Deepgram.Models.PreRecorded.v1;

public record TopicGroup
{
    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("segments")]
    public IReadOnlyList<Segment>? Segments { get; set; }
}

