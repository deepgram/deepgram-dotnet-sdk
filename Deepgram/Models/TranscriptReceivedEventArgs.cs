using System;

namespace Deepgram.Models
{
    public class TranscriptReceivedEventArgs : EventArgs
    {
        public TranscriptReceivedEventArgs(LiveTranscriptionResult transcript)
        {
            Transcript = transcript;
        }

        public LiveTranscriptionResult Transcript { get; set; }
    }
}
