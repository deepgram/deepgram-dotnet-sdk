namespace Deepgram.Models
{
    public class AppSettings
    {

        /// <summary>
        /// Deepgram API Key
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// On-premise Url of the Deepgram API
        /// </summary>
        public string ApiUrl { get; set; } = string.Empty;

        /// <summary>
        /// Require SSL on requests
        /// </summary>
        public string RequireSSL { get; set; } = null;

        /// <summary>
        /// Request Per second for HttpClient
        /// </summary>
        public double RequestsPerSecond { get; set; }


    }
}
