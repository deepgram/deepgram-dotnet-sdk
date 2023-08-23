using Deepgram.Interfaces;
using Deepgram.Models;
using Deepgram.Utilities;

namespace Deepgram.Clients
{
    public class TranscriptionClient : ITranscriptionClient
    {
        public IPrerecordedTranscriptionClient Prerecorded { get; protected set; }

        public TranscriptionClient(Credentials credentials, HttpClientUtil httpClientUtil)
        {
            Prerecorded = new PrerecordedTranscriptionClient(credentials, httpClientUtil);
        }


    }
}
