namespace Deepgram.CustomEventArgs
{
    public class TranscriptReceivedEventArgs : EventArgs
    {
        public LiveTranscriptionResponse Transcript { get; set; }
        public TranscriptReceivedEventArgs(LiveTranscriptionResponse transcript)
        {
            Transcript = transcript;
        }


    }
}
