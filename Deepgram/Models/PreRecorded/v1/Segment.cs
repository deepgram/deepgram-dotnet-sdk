namespace Deepgram.Models.PreRecorded.v1;

public class Segment
{
    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("start_word")]
    public int? StartWord { get; set; }

    [JsonPropertyName("end_word")]
    public int? EndWord { get; set; }

    [JsonPropertyName("sentiment")]
    public string Sentiment { get; set; }

    [JsonPropertyName("sentiment_score")]
    public double? SentimentScore { get; set; }
}
