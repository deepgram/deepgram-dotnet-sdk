using Deepgram.Models.Live.v1;

namespace Deepgram.DeepgramEventArgs
{
    public class LiveResponseReceivedEventArgs(LiveResponse response) : EventArgs
    {
        public LiveResponse Response { get; set; } = response;
    }
}
