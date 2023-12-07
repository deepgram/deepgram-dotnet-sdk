namespace Deepgram.Records.PreRecorded;

public record WordBase
{
    [JsonPropertyName("word")]
    public string Word { get; set; }

    [JsonPropertyName("start")]
    public double Start { get; set; }

    [JsonPropertyName("end")]
    public double End { get; set; }

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    [JsonPropertyName("punctuated_word")]
    public string PunctuatedWord { get; set; }

    [JsonPropertyName("speaker")]
    public int? Speaker { get; set; }

    [JsonPropertyName("speaker_confidence")]
    public double? SpeakerConfidence { get; set; }
}

