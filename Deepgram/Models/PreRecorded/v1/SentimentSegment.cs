namespace Deepgram.Models.PreRecorded.v1;

public class SentimentSegment
{

    [JsonPropertyName("sentiment")]
    public string Sentiment { get; set; }


    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }


    [JsonPropertyName("text")]
    public string Text { get; set; }


    [JsonPropertyName("start_word")]
    public int? StartWord { get; set; }


    [JsonPropertyName("end_word")]
    public int? EndWord { get; set; }
}


