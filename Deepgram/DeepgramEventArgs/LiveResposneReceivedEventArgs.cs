using Deepgram.Records.Live;

namespace Deepgram.DeepgramEventArgs
{
    public class LiveResponseReceivedEventArgs(LiveResponse response) : EventArgs
    {
        public LiveResponse Response { get; set; } = response;
    }
}
