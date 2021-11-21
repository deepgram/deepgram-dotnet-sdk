using System;

namespace Deepgram.Request
{
    internal class CleanCredentials
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="apiKey">Deepgram API Key</param>
        /// <param name="apiUrl">Uri of Deepgram API</param>
        public CleanCredentials(string apiKey, Uri apiUrl)
        {
            ApiKey = apiKey;
            ApiUrl = apiUrl;
        }

        /// <summary>
        /// Deepgram API Key
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// On-premise Url of the Deepgram API
        /// </summary>
        public Uri ApiUrl { get; set; }

        public Credentials ToCredentials()
        {
            return new Credentials(ApiKey, ApiUrl.ToString());
        }

    }
}
