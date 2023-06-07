using System;
using Deepgram.Clients;
using Deepgram.Extensions;
using Deepgram.Interfaces;
using Deepgram.Models;
using Deepgram.Request;

namespace Deepgram
{
    public class DeepgramClient
    {
        public IKeyClient Keys { get; private set; }
        public IProjectClient Projects { get; private set; }
        public ITranscriptionClient Transcription { get; private set; }
        public IUsageClient Usage { get; private set; }

        private TimeSpan Timeout = TimeSpan.Zero;

        private Credentials Credentials;


        public DeepgramClient() : this(null)
        {
        }

        public DeepgramClient(Credentials credentials)
        {
            Initialize(credentials);
        }

        private void Initialize(Credentials credentials)
        {
            Credentials = CredentialsExtension.Clean(credentials);
            InitializeClients();
        }

        private void InitializeClients()
        {
            var apiRequest = new ApiRequest(Credentials, Timeout);
            Keys = new KeyClient(apiRequest);
            Projects = new ProjectClient(apiRequest);
            Transcription = new TranscriptionClient(apiRequest);
            Usage = new UsageClient(apiRequest);
        }


        public ILiveTranscriptionClient CreateLiveTranscriptionClient()
        {
            return new LiveTranscriptionClient(Credentials);
        }

        /// <summary>
        /// Sets the Timeout of the HTTPClient used to send HTTP requests
        /// </summary>
        /// <param name="timeout">Timespan to wait before the request times out.</param>
        public void SetHttpClientTimeout(TimeSpan timeout)
        {
            Timeout = timeout;
            InitializeClients();
        }
    }
}
