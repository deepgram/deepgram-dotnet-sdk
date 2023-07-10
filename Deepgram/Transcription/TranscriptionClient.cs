using Deepgram.Request;

namespace Deepgram.Transcription
{
    public class TranscriptionClient : ITranscriptionClient
    {
        public IPrerecordedTranscriptionClient Prerecorded { get; internal set; }
        public TranscriptionClient(ApiRequest apiRequest)
        {
            Prerecorded = new PrerecordedTranscriptionClient(apiRequest);
        }

    }
}
