using System;
using Deepgram.Clients;
using Deepgram.Common;
using Deepgram.Interfaces;
using Deepgram.Models;


namespace Deepgram
{
    public class DeepgramClient
    {
        private CleanCredentials _credentials;

        public Credentials Credentials
        {
            get => _credentials.ToCredentials();
            set
            {
                InitializeCredentials(value);
                InitializeClients();
            }
        }

        public DeepgramClient() : this(null)
        {
        }

        public DeepgramClient(Credentials credentials)
        {
            InitializeCredentials(credentials);
            InitializeClients();
        }

        /// <summary>
        /// Sets the Timeout of the HTTPClient used to send HTTP requests
        /// </summary>
        /// <param name="timeout">Timespan to wait before the request times out.</param>
        public void SetHttpClientTimeout(TimeSpan timeout)
        {
            Configuration.Instance.Client.Timeout = timeout;
        }

            //Set values and clean them up 
            _credentials = new Credentials(
                CleanCredentials.CheckApiKey(credentials.ApiKey),
                CleanCredentials.CleanApiUrl(credentials.ApiUrl),
                CleanCredentials.CleanRequireSSL(credentials.RequireSSL));
        }

        private void InitializeClients()
        {
            Keys = new KeyClient(_credentials);
            Projects = new ProjectClient(_credentials);
            Transcription = new TranscriptionClient(_credentials);
            Usage = new UsageClient(_credentials);
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
