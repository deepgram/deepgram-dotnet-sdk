namespace Deepgram.Models;

public class Entity
{
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    [JsonPropertyName("start_word")]
    public int? StartWord { get; set; }

    [JsonPropertyName("end_word")]
    public int? EndWord { get; set; }
}