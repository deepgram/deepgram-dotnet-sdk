namespace Deepgram.Records.PreRecorded;

public class Topic
{
    /// <summary>
    /// Topic detected.
    /// </summary>
    [JsonPropertyName("topic")]
    public string? Name { get; set; }

    /// <summary>
    /// Value between 0 and 1 indicating the model's relative confidence in this topic.
    /// </summary>
    [JsonPropertyName("confidence")]
    public decimal? Confidence { get; set; }
}

