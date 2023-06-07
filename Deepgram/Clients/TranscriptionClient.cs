using Deepgram.Interfaces;
using Deepgram.Models;

namespace Deepgram.Clients
{
    internal class TranscriptionClient : ITranscriptionClient
    {
        private Credentials _credentials;

        public TranscriptionClient(Credentials credentials)
        {
            _credentials = credentials;
            InitializeClients();
        }

        public IPrerecordedTranscriptionClient Prerecorded { get; private set; }

        private void InitializeClients()
        {
            Prerecorded = new PrerecordedTranscriptionClient(_credentials);
        }
    }
}
