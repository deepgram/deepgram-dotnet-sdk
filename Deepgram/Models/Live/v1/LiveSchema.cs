using Deepgram.Models.Shared.v1;

namespace Deepgram.Models.Live.v1;

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
    /// <see href="https://developers.deepgram.com/docs/encoding">
    /// supported encodings <see cref="AudioEncoding"/>
    /// </summary>
    [JsonPropertyName("encoding")]
    public string? Encoding { get; set; }

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

    [JsonPropertyName("numerals")]
    [Obsolete("Replaced with SmartFormat")]
    public bool? Numerals { get; set; }
    /// <summary>
    /// Sample Rate allows you to specify the sample rate of your submitted audio.
    /// <see href="https://developers.deepgram.com/docs/sample-rate">
    /// only applies when Encoding has a value
    /// </summary>
    [JsonPropertyName("sample_rate")]
    public int? SampleRate { get; set; }

    /// <summary>
    /// Indicates how long Deepgram will wait to send a {"type": "UtteranceEnd"} message after a word has been transcribed
    /// <see href="https://developers.deepgram.com/docs/understanding-end-of-speech-detection-while-streaming"/>
    /// </summary>
    [JsonPropertyName("utterance_end_ms")]
    public int? UtteranceEnd { get; set; }

    /// <summary>
    /// This is achieved through a Voice Activity Detector (VAD), which gauges the tonal nuances of human speech and can better differentiate between silent and non-silent audio.
    /// <see href="https://developers.deepgram.com/docs/start-of-speech-detection"/>
    /// </summary>
    [JsonPropertyName("vad_events")]
    public bool? DetectSpeechStart { get; set; }
}
