namespace Deepgram.Records.PreRecorded;

public class Topic
{
    /// <summary>
    /// Value between 0 and 1 indicating the model's relative confidence in this topic.
    /// </summary>
    [JsonPropertyName("confidence")]
    public decimal? Confidence { get; set; }

    [JsonPropertyName("end_word")]
    public int? EndWord;

    /// <summary>
    /// Topic detected.
    /// </summary>
    [JsonPropertyName("topic")]
    public string? Name { get; set; }

    [JsonPropertyName("start_word")]
    public int? StartWord;

    [JsonPropertyName("text")]
    public string Text;

    [JsonPropertyName("topics")]
    public List<Topic> Topics;
}

