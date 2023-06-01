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

        public IKeyClient Keys { get; private set; }
        public IProjectClient Projects { get; private set; }
        public ITranscriptionClient Transcription { get; private set; }
        public IUsageClient Usage { get; private set; }

        public ILiveTranscriptionClient CreateLiveTranscriptionClient()
        {
            return new LiveTranscriptionClient(_credentials);
        }

        private void InitializeCredentials(Credentials credentials = null)
        {
            string apiUrl = string.IsNullOrWhiteSpace(credentials?.ApiUrl) ? "" : credentials.ApiUrl;
            string apiKey = string.IsNullOrWhiteSpace(credentials?.ApiKey) ? "" : credentials.ApiKey;
            Nullable<bool> requireSSL = credentials?.RequireSSL;

            if (string.IsNullOrEmpty(apiKey))
            {
                string possibleApiKey = Configuration.Instance.Settings[Constants.API_KEY_SECTION];
                if (!string.IsNullOrEmpty(possibleApiKey))
                {
                    apiKey = possibleApiKey;
                }
                else
                {
                    throw new ArgumentException("Deepgram API Key must be provided in constructor or via settings");
                }
            }
            if (string.IsNullOrEmpty(apiUrl))
            {
                string possibleUri = Configuration.Instance.Settings[Constants.API_URI_SECTION];
                if (string.IsNullOrEmpty(possibleUri))
                {
                    apiUrl = "api.deepgram.com";
                }
                else
                {
                    apiUrl = possibleUri;
                }
            }
            if (!requireSSL.HasValue)
            {
                string possibleRequireSSL = Configuration.Instance.Settings[Constants.API_REQUIRE_SSL];
                if (string.IsNullOrEmpty(possibleRequireSSL))
                {
                    requireSSL = true;
                }
                else
                {
                    requireSSL = Convert.ToBoolean(possibleRequireSSL);
                }
            }
            _credentials = new CleanCredentials(apiKey, apiUrl, requireSSL.Value);
        }

        private void InitializeClients()
        {
            Keys = new KeyClient(_credentials);
            Projects = new ProjectClient(_credentials);
            Transcription = new TranscriptionClient(_credentials);
            Usage = new UsageClient(_credentials);
        }
    }
}
