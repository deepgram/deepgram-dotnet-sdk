namespace Deepgram.Models.Live.v1;
public class LiveResponse
{
    /// <summary>
    /// MetaData response from the live transcription service
    /// </summary>
    public LiveMetadataResponse? MetaData { get; set; }
    /// <summary>
    /// Transcription response from the live transcription service
    /// </summary>
    public LiveTranscriptionResponse? Transcription { get; set; }
    /// <summary>
    /// UtterancEnd response from the live transcription service
    /// </summary>
    public LiveUtteranceEndResponse? UtteranceEnd { get; set; }
    /// <summary>
    /// Error response from the live transcription service
    /// </summary>
    public Exception? Error { get; set; }
}
