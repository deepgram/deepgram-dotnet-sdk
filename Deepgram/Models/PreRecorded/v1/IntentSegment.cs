namespace Deepgram.Models.PreRecorded.v1;

public record IntentSegment
{
    /// <summary>
    /// <see href="https://developers.deepgram.com/reference/audio-intelligence-apis#intent-recognition"/>
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// <see href="https://developers.deepgram.com/reference/audio-intelligence-apis#intent-recognition"/>
    /// </summary>
    [JsonPropertyName("start_word")]
    public int? StartWord { get; set; }

    /// <summary>
    /// <see href="https://developers.deepgram.com/reference/audio-intelligence-apis#intent-recognition"/>   
    /// </summary>
    [JsonPropertyName("end_word")]
    public int? EndWord { get; set; }

    /// <summary>   
    /// <see href="https://developers.deepgram.com/reference/audio-intelligence-apis#intent-recognition"/>  
    /// <see cref="Intent"/>
    /// </summary>
    [JsonPropertyName("intents")]
    public IReadOnlyList<Intent>? Intents { get; set; }
}
