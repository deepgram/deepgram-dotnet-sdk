namespace Deepgram.Models
{
    internal class CleanCredentials
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="apiKey">Deepgram API Key</param>
        /// <param name="apiUrl">Url of Deepgram API</param>
        /// <param name="requireSSL">Require SSL on requests</param>
        public CleanCredentials(string apiKey, string apiUrl, bool requireSSL)
        {
            ApiKey = apiKey;

            // Remove scheme from apiUrl. We'll append the correct
            // scheme based on the type of request.
            if (apiUrl.Contains("://"))
            {
                apiUrl = apiUrl.Substring(apiUrl.IndexOf("://") + 3);
            }

            ApiUrl = apiUrl;
            RequireSSL = requireSSL;
        }

        /// <summary>
        /// Deepgram API Key
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// On-premise Url of the Deepgram API
        /// </summary>
        public string ApiUrl { get; set; }

        /// <summary>
        /// Require SSL on requests
        /// </summary>
        public bool RequireSSL { get; set; }

        public Credentials ToCredentials()
        {
            return new Credentials(ApiKey, ApiUrl, RequireSSL);
        }
    }
}
