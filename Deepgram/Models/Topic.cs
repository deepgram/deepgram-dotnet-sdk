namespace Deepgram.Models;

public class Topic
{
    [JsonPropertyName("topic")]
    public string? Name { get; set; }

    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }
}

