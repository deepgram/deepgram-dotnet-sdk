namespace Deepgram.Models.PreRecorded.v1;

public class Segment
{
    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("start_word")]
    public int? StartWord { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("end_word")]
    public int? EndWord { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("sentiment")]
    public string? Sentiment { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("sentiment_score")]
    public double? SentimentScore { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("topics")]
    public IReadOnlyList<Topic>? Topics { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("intents")]
    public IReadOnlyList<Intent>? Intents { get; set; }
}
