using Deepgram.Records.Live;

namespace Deepgram.DeepgramEventArgs
{
    public class UtteranceEndReceivedEventArgs(LiveUtteranceEndResponse utteranceEnd) : EventArgs
    {
        public LiveUtteranceEndResponse UtteranceEnd { get; set; } = utteranceEnd;
    }
}
