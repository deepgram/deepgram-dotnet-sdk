namespace Deepgram.Models.Live.v1;

public class EventResponse
{
    /// <summary>
    /// MetaData response from the live transcription service
    /// </summary>
    public MetadataResponse? MetaData { get; set; }

    /// <summary>
    /// Transcription response from the live transcription service
    /// </summary>
    public TranscriptionResponse? Transcription { get; set; }

    /// <summary>
    /// UtterancEnd response from the live transcription service
    /// </summary>
    public UtteranceEndResponse? UtteranceEnd { get; set; }

    /// <summary>
    /// Error response from the live transcription service
    /// </summary>
    public Exception? Error { get; set; }
}
