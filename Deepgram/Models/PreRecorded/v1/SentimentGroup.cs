namespace Deepgram.Models.PreRecorded.v1;

public class SentimentGroup
{
    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("segments")]
    public IReadOnlyList<Segment>? Segments { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("average")]
    public Average? Average { get; set; }
}


