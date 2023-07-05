namespace Deepgram.Models;

public class LiveTranscriptionResult
{
    /// <summary>
    /// Information about the active channel in the form 
    /// [channel_index, total_number_of_channels].
    /// </summary>
    [JsonPropertyName("channel_index")]
    public int[] ChannelIndex { get; set; }

    /// <summary>
    /// Duration in seconds.
    /// </summary>
    [JsonPropertyName("duration")]
    public decimal Duration { get; set; }

    /// <summary>
    /// Offset in seconds.
    /// </summary>
    [JsonPropertyName("start")]
    public decimal Start { get; set; }

    /// <summary>
    /// Indicates that Deepgram has identified a point at which its 
    /// transcript has reached maximum accuracy and is sending a 
    /// definitive transcript of all audio up to that point.
    /// </summary>
    [JsonPropertyName("is_final")]
    public bool IsFinal { get; set; }

    /// <summary>
    /// Indicates that Deepgram has detected an endpoint and immediately 
    /// finalized its results for the processed time range.
    /// </summary>
    [JsonPropertyName("speech_final")]
    public bool SpeechFinal { get; set; }

    /// <summary>
    /// Transcript of the channel
    /// </summary>
    [JsonPropertyName("channel")]
    public Channel Channel { get; set; }
}
