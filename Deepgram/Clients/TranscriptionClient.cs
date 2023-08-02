using Deepgram.Interfaces;
using Deepgram.Models;

namespace Deepgram.Clients
{
    public class TranscriptionClient : ITranscriptionClient
    {
        public IPrerecordedTranscriptionClient Prerecorded { get; protected set; }

        public TranscriptionClient(Credentials credentials)
        {
            Prerecorded = new PrerecordedTranscriptionClient(credentials);
        }


    }
}
