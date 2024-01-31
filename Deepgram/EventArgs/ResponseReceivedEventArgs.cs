using Deepgram.Models.Live.v1;

namespace Deepgram.DeepgramEventArgs;

public class ResponseReceivedEventArgs(EventResponse response) : EventArgs
{
    public EventResponse Response { get; set; } = response;
}

