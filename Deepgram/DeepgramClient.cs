using System;
using Deepgram.Clients;
using Deepgram.Common;
using Deepgram.Interfaces;
using Deepgram.Models;

namespace Deepgram
{
    public class DeepgramClient
    {
        public IKeyClient Keys { get; private set; }
        public IProjectClient Projects { get; private set; }
        public ITranscriptionClient Transcription { get; private set; }
        public IUsageClient Usage { get; private set; }


        public Credentials _credentials;
        public Credentials Credentials
        {
            get => _credentials;
            set
            {
                Initialize(value);
            }
        }

        public DeepgramClient() : this(null)
        {
        }

        public DeepgramClient(Credentials credentials)
        {
            Initialize(credentials);
        }

        private void Initialize(Credentials credentials)
        {
            InitializeCredentials(credentials);
            InitializeClients();
        }

        private void InitializeCredentials(Credentials credentials = null)
        {
            //if no credentials are passed in the constructor create a empty credentials
            if (credentials == null)
                _credentials = new Credentials();

            //Set values and clean them up 
            _credentials = new Credentials(
                CleanCredentials.CheckApiKey(credentials.ApiKey),
                CleanCredentials.CleanApiUrl(credentials.ApiUrl),
                CleanCredentials.CleanRequireSSL(credentials.RequireSSL));
        }

        public void SetHttpClientTimeout(TimeSpan timeout)
        {
            TimeoutSingleton.Instance.Timeout = timeout;
        }
        public ILiveTranscriptionClient CreateLiveTranscriptionClient()
        {
            return new LiveTranscriptionClient(_credentials);
        }

        /// <summary>
        /// Sets the Timeout of the HTTPClient used to send HTTP requests
        /// </summary>
        /// <param name="timeout">Timespan to wait before the request times out.</param>


        public void SetHttpClientTimeout(TimeSpan timeout)
        {
            TimeoutSingleton.Instance.Timeout = timeout;
        }
    }
}
