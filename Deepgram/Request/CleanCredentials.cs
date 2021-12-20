using System;

namespace Deepgram.Request
{
    internal class CleanCredentials
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="apiKey">Deepgram API Key</param>
        /// <param name="apiUrl">Url of Deepgram API</param>
        public CleanCredentials(string apiKey, string apiUrl)
        {
            ApiKey = apiKey;

            // Remove scheme from apiUrl. We'll append the correct
            // scheme based on the type of request.
            if (apiUrl.Contains("://"))
            {
                apiUrl = apiUrl.Substring(apiUrl.IndexOf("://") + 3);
            }

            ApiUrl = apiUrl;
        }

        /// <summary>
        /// Deepgram API Key
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// On-premise Url of the Deepgram API
        /// </summary>
        public string ApiUrl { get; set; }

        public Credentials ToCredentials()
        {
            return new Credentials(ApiKey, ApiUrl);
        }

    }
}
