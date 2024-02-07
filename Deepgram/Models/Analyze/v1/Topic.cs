namespace Deepgram.Models.Analyze.v1;

public record Topic
{
    /// <summary>
    /// Value between 0 and 1 indicating the model's relative confidence in this topic.
    /// </summary>
    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("topic")]
    public string Text;
}

