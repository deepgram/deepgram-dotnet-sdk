using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Deepgram.Request;

namespace Deepgram.Transcription
{
    internal class TranscriptionClient: ITranscriptionClient
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
