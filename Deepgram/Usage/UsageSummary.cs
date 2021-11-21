using System;
using System.Text.Json.Serialization;

namespace Deepgram.Usage
{
    public class UsageSummary
    {
        /// <summary>
        /// Start date for included requests.
        /// </summary>
        [JsonPropertyName("start")]
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// End date for included requests.
        /// </summary>
        [JsonPropertyName("end")]
        public DateTime EndDateTime { get; set; }

        /// <summary>
        /// Resolution of the usage
        /// </summary>
        [JsonPropertyName("resolution")]
        public UsageSummaryResolution Resolution { get; set; }

        /// <summary>
        /// Result summaries
        /// </summary>
        [JsonPropertyName("results")]
        public List<UsageSummaryResult> Results { get; set; }
    }
}
