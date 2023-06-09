using Deepgram.Interfaces;
using Deepgram.Request;

namespace Deepgram.Clients
{
    internal class TranscriptionClient : ITranscriptionClient
    {
        private CleanCredentials _credentials;

        public TranscriptionClient(CleanCredentials credentials)
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
