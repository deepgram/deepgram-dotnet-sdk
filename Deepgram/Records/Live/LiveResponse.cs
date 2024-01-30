namespace Deepgram.Records.Live;
public class LiveResponse
{
    public LiveMetadataResponse? MetaData { get; set; }
    public LiveTranscriptionResponse? Transcription { get; set; }
    public LiveUtteranceEndResponse? UtteranceEnd { get; set; }
    public Exception? Error { get; set; }
}
