using System;
using Deepgram.Clients;
using Deepgram.Interfaces;
using Deepgram.Models;
using Deepgram.Utilities;

namespace Deepgram
{
    public class DeepgramClient
    {
        private Credentials Credentials;

        public IKeyClient Keys { get; protected set; }
        public IProjectClient Projects { get; protected set; }
        public ITranscriptionClient Transcription { get; protected set; }
        public IUsageClient Usage { get; protected set; }
        public IBillingClient Billing { get; protected set; }

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
            HttpClientUtil.SetTimeOut(timeout);

        private void Initialize(Credentials credentials)
        {
            Credentials = CredentialsUtil.Clean(credentials);
            InitializeClients();
        }

        protected void InitializeClients()
        {

            Keys = new KeyClient(Credentials);
            Projects = new ProjectClient(Credentials);
            Transcription = new TranscriptionClient(Credentials);
            Usage = new UsageClient(Credentials);
            Billing = new BillingClient(Credentials);

        }
    }
}
