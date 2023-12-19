namespace Deepgram.Records;

public record Intent
{
    /// <summary>
    /// <see href="https://developers.deepgram.com/reference/audio-intelligence-apis#intent-recognition"/>
    /// </summary>
    [JsonPropertyName("intent")]
    public string Intention { get; set; }
    /// <summary>
    /// <see href="https://developers.deepgram.com/reference/audio-intelligence-apis#intent-recognition"/>
    /// </summary>
    [JsonPropertyName("confidence_score")]
    public double ConfidenceScore { get; set; }
}



