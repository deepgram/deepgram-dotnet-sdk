using Deepgram.Records.Live;

namespace Deepgram.DeepgramEventArgs
{
    public class TranscriptReceivedEventArgs(LiveTranscriptionEvent transcript) : EventArgs
    {
        public LiveTranscriptionEvent Transcript { get; set; } = transcript;
    }
}
