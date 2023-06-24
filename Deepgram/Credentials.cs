using System;

namespace Deepgram
{
    public class Credentials
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="apiKey">Deepgram API Key</param>
        /// <param name="apiUrl">Url of Deepgram API</param>
        /// <param name="requireSSL">Require SSL on requests</param>
        public Credentials(string apiKey = null, string apiUrl = null, bool requireSSL = true)
        {
            ApiKey = apiKey;
            ApiUrl = apiUrl;
            RequireSSL = requireSSL;
        }

        /// <summary>
        /// Deepgram API Key
        /// </summary>
        public string ApiKey { get; set; } = null;

        /// <summary>
        /// On-premise Url of the Deepgram API
        /// </summary>
        public string ApiUrl { get; set; } = null;

        /// <summary>
        /// Require SSL on requests
        /// </summary>
        public Nullable<bool> RequireSSL { get; set; } = null;
    }
}
