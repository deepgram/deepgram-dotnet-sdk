namespace Deepgram.Models;

public class Sentence
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("start")]
    public int? Start { get; set; }

    [JsonPropertyName("end")]
    public int? End { get; set; }
}

