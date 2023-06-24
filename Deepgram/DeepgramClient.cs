using System;
using Deepgram.Clients;
using Deepgram.Extensions;
using Deepgram.Interfaces;
using Deepgram.Request;

namespace Deepgram
{
    public class DeepgramClient
    {
        private Credentials Credentials;
        public IKeyClient Keys { get; private set; }
        public IProjectClient Projects { get; private set; }
        public ITranscriptionClient Transcription { get; private set; }
        public IUsageClient Usage { get; private set; }
        public ILiveTranscriptionClient CreateLiveTranscriptionClient() => new LiveTranscriptionClient(Credentials);
        public DeepgramClient() : this(null) { }

        public DeepgramClient(Credentials credentials)
        {
            Initialize(credentials);
        }

        /// <summary>
        /// Sets the Timeout of the HTTPClient used to send HTTP requests
        /// </summary>
        /// <param name="timeout">Timespan to wait before the request times out.</param>
        public void SetHttpClientTimeout(TimeSpan timeout) =>
            ApiRequest.SetTimeOut(timeout);

        private void Initialize(Credentials credentials)
        {
            Credentials = CredentialsExtension.Clean(credentials);
            InitializeClients();
        }

        private void InitializeClients()
        {

            Keys = new KeyClient(Credentials);
            Projects = new ProjectClient(Credentials);
            Transcription = new TranscriptionClient(Credentials);
            Usage = new UsageClient(Credentials);
        }
    }
}
