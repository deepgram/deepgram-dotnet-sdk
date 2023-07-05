namespace Deepgram.Models;

public class PrerecordedTranscriptionResult
{
    /// <summary>
    /// Array of Channel objects.
    /// </summary>
    [JsonPropertyName("channels")]
    public Channel[] Channels { get; set; }

    /// <summary>
    /// Array of Utterance objects. 
    /// </summary>
    [JsonPropertyName("utterances")]
    public Utterance[] Utterances { get; set; }

    /// <summary>
    /// Summary of Transcription.
    /// </summary>
    [JsonPropertyName("summary")]
    public Summary Summary { get; set; }
}
