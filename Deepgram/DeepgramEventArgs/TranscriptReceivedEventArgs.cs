namespace Deepgram.DeepgramEventArgs
{
    public class TranscriptReceivedEventArgs(LiveTranscriptionResponse transcript) : EventArgs
    {
        public LiveTranscriptionResponse Transcript { get; set; } = transcript;
    }
}
