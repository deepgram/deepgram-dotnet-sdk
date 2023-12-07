namespace Deepgram.Records.Live;

public record Alternative
{
    [JsonPropertyName("transcript")]
    public string? Transcript { get; set; }

    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    [JsonPropertyName("words")]
    public IEnumerable<Word> Words { get; set; }


}
