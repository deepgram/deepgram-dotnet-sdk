namespace Deepgram.Models.Analyze.v1;

public record SentimentGroup
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


