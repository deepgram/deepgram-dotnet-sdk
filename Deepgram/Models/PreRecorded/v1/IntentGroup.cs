namespace Deepgram.Models.PreRecorded.v1;

public record IntentGroup
{
    /// <summary> 
    /// <see href="https://developers.deepgram.com/reference/audio-intelligence-apis#intent-recognition"/>
    /// <see cref="IntentSegment"/>
    /// </summary>
    [JsonPropertyName("segments")]
    public IReadOnlyList<IntentSegment>? Segments { get; set; }
}
