namespace Deepgram.Records;
public record Intents
{
    /// <summary> 
    /// <see href="https://developers.deepgram.com/reference/audio-intelligence-apis#intent-recognition"/>
    /// <see cref="Segment"/>
    /// </summary>
    [JsonPropertyName("segments")]
    public IReadOnlyList<Segment>? Segments { get; set; }

}
