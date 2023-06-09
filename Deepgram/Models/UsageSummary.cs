using Newtonsoft.Json;
using System;

namespace Deepgram.Models
{
    public class UsageSummary
    {
        /// <summary>
        /// Start date for included requests.
        /// </summary>
        [JsonProperty("start")]
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// End date for included requests.
        /// </summary>
        [JsonProperty("end")]
        public DateTime EndDateTime { get; set; }

        /// <summary>
        /// Resolution of the usage
        /// </summary>
        [JsonProperty("resolution")]
        public UsageSummaryResolution Resolution { get; set; }

        /// <summary>
        /// Result summaries
        /// </summary>
        [JsonProperty("results")]
        public UsageSummaryResult[] Results { get; set; }
    }
}
