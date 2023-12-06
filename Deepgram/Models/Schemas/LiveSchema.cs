namespace Deepgram.Models.Schemas;

public class LiveSchema : TranscriptionSchema
{
    /// <summary>
    /// Channels allows you to specify the number of independent audio channels your submitted audio contains. 
    /// Used when the Encoding feature is also being used to submit streaming raw audio
    /// <see href="https://developers.deepgram.com/docs/channels">
    /// </summary>
    [JsonPropertyName("channels")]
    public int? Channels { get; set; }

    /// <summary>
    /// Encoding allows you to specify the expected encoding of your submitted audio.
    /// <see href="https://developers.deepgram.com/docs/">
    /// </summary>
    [JsonPropertyName("encoding")]
    public string? Encoding { get; set; }

    /// <summary>
    /// Sample Rate allows you to specify the sample rate of your submitted audio.
    /// <see href="https://developers.deepgram.com/docs/sample-rate">
    /// </summary>
    [JsonPropertyName("sample_rate")]
    public int? SampleRate { get; set; }

    /// <summary>
    /// Endpointing returns transcripts when pauses in speech are detected.
    /// <see href="https://developers.deepgram.com/docs/endpointing">
    /// </summary>
    [JsonPropertyName("endpointing")]
    public string? EndPointing { get; set; }

    /// <summary>
    /// Interim Results provides preliminary results for streaming audio to solve the need for immediate results combined with high levels of accuracy.
    /// <see href="https://developers.deepgram.com/docs/interim-results">
    /// </summary>
    [JsonPropertyName("interim_results")]
    public bool? InterimResults { get; set; }
}
