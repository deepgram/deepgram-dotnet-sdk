using Deepgram.Records.Live;

namespace Deepgram.DeepgramEventArgs
{
    public class MetadataReceivedEventArgs(LiveMetadataResponse metadata) : EventArgs
    {
        public LiveMetadataResponse MetaData { get; set; } = metadata;
    }
}
