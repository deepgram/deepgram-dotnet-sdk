namespace Deepgram.Models;

public class Paragraph
{
    [JsonPropertyName("sentences")]
    public Sentence[]? Sentences { get; set; }

    [JsonPropertyName("start")]
    public int? Start { get; set; }

    [JsonPropertyName("end")]
    public int? End { get; set; }

    [JsonPropertyName("num_words")]
    public int? NumWords { get; set; }
}

