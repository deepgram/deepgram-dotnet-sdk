using System;
using Deepgram.Billing;
using Deepgram.Invitation;
using Deepgram.Keys;
using Deepgram.Projects;
using Deepgram.Request;
using Deepgram.Transcription;
using Deepgram.Usage;
using Deepgram.Utilities;

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
        public void SetHttpClientTimeout(TimeSpan timeout) =>
            HttpClientUtil.SetTimeOut(timeout);


        public IKeyClient Keys { get; private set; }
        public IProjectClient Projects { get; private set; }
        public ITranscriptionClient Transcription { get; private set; }
        public IUsageClient Usage { get; private set; }
        public IBillingClient Billing { get; private set; }
        public IInvitationClient Invitation { get; private set; }
        public ILiveTranscriptionClient CreateLiveTranscriptionClient()
        {
            return new LiveTranscriptionClient(_credentials);
        }

        private void InitializeCredentials(Credentials credentials = null)
        {
            _credentials = CredentialsUtil.Clean(credentials);
        }

        private void InitializeClients()
        {
            Keys = new KeyClient(_credentials);
            Projects = new ProjectClient(_credentials);
            Transcription = new TranscriptionClient(_credentials);
            Usage = new UsageClient(_credentials);
            Billing = new BillingClient(_credentials);
            Invitation = new InvitationClient(_credentials);
        }
    }
}
