namespace Deepgram.Models;

public class Topic
{
    /// <summary>
    /// Topic detected.
    /// </summary>
    [JsonProperty("topic")]
    public string TopicText { get; set; }

    /// <summary>
    /// Value between 0 and 1 indicating the model's relative confidence in this topic.
    /// </summary>
    [JsonProperty("confidence")]
    public decimal Confidence { get; set; }
}
