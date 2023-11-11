namespace Deepgram.Models;
public class LiveTranscriptionEvent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "Results";

    [JsonPropertyName("channel_index")]
    public int[]? ChannelIndex { get; set; }

    [JsonPropertyName("duration")]
    public double? Duration { get; set; }

    [JsonPropertyName("start")]
    public double? Start { get; set; }

    [JsonPropertyName("is_final")]
    public bool? IsFinal { get; set; }

    [JsonPropertyName("speech_final")]
    public bool? SpeechFinal { get; set; }

    [JsonPropertyName("channel")]
    public Channel? Channel { get; set; }

    [JsonPropertyName("metadata")]
    public LiveMetaData? MetaData { get; set; }
}
