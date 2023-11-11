namespace Deepgram.Enums;
public enum LiveTranscriptionEvent
{
    Open,
    Close,
    Transcript, // exact match to data type from API
    Metadata, // exact match to data type from API
    Error,
    Warning,
}
