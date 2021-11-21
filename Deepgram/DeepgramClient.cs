using System;
using Deepgram.Keys;
using Deepgram.Projects;
using Deepgram.Request;
using Deepgram.Transcription;
using Deepgram.Usage;

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

        public DeepgramClient()
        {
            InitializeCredentials();
            InitializeClients();
        }

        public DeepgramClient(Credentials credentials)
        {
            InitializeCredentials(credentials);
            InitializeClients();
        }

        public IKeyClient Keys { get; private set; }
        public IProjectClient Projects { get; private set; }
        public ITranscriptionClient Transcription { get; private set; }
        public IUsageClient Usage { get; private set; }

        private void InitializeCredentials(Credentials? credentials = null)
        {
            string apiUrl = string.IsNullOrWhiteSpace(credentials?.ApiUrl) ? "" : credentials.ApiUrl;
            string apiKey = string.IsNullOrWhiteSpace(credentials?.ApiKey) ? "" : credentials.ApiKey;

            if (string.IsNullOrEmpty(apiKey))
            {
                string possibleApiKey = Configuration.Instance.Settings["appSettings:Deepgram.Api.Key"];
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
                string possibleUri = Configuration.Instance.Settings["appSettings:Deepgram.Api.Uri"];
                if (string.IsNullOrEmpty(possibleUri))
                {
                    apiUrl = "https://api.deepgram.com";
                }
                else
                {
                    apiUrl = possibleUri;
                }
            }
            _credentials = new CleanCredentials(apiKey, new Uri(apiUrl));
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
