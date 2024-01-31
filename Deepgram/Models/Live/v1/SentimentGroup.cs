namespace Deepgram.Models.Live.v1;

public class SentimentGroup
{

    [JsonPropertyName("sentiment")]
    public string Sentiment { get; set; }


    [JsonPropertyName("sentiment_score")]
    public double? SentimentScore { get; set; }


    [JsonPropertyName("start_time")]
    public double? StartTime { get; set; }


    [JsonPropertyName("end_time")]
    public double? EndTime { get; set; }


    [JsonPropertyName("segments")]
    public List<Segment> Segments { get; } = [];


    [JsonPropertyName("average")]
    public Average Average { get; set; }
}


