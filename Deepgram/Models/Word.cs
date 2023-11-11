namespace Deepgram.Models;

public class Word
{
    [JsonPropertyName("word")]
    public string? Name { get; set; }

    [JsonPropertyName("start")]
    public double? Start { get; set; }

    [JsonPropertyName("end")]
    public double? End { get; set; }

    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    [JsonPropertyName("punctuated_word")]
    public string? PunctuatedWord { get; set; }
}
