using Deepgram.Interfaces;
using Deepgram.Request;

namespace Deepgram.Clients
{
    internal class TranscriptionClient : ITranscriptionClient
    {
        private ApiRequest _apiRequest;

        public TranscriptionClient(ApiRequest apiRequest)
        {
            _apiRequest = apiRequest;
            InitializeClients();
        }

        public IPrerecordedTranscriptionClient Prerecorded { get; private set; }

        private void InitializeClients()
        {
            Prerecorded = new PrerecordedTranscriptionClient(_apiRequest);
        }
    }
}
