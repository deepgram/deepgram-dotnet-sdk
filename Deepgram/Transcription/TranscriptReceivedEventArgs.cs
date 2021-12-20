using System;

namespace Deepgram.Transcription
{
    public class TranscriptReceivedEventArgs: EventArgs
    {
        public TranscriptReceivedEventArgs(LiveTranscriptionResult transcript)
        {
            Transcript = transcript;
        }

         public LiveTranscriptionResult Transcript { get; set; }
    }
}
