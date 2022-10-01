using System;
using Newtonsoft.Json;

namespace Deepgram.Usage
{
    public class UsageSummaryResult
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
        /// Length of time (in hours) of audio submitted in included requests.
        /// </summary>
        [JsonProperty("hours")]
        public decimal Hours { get; set; }

        /// <summary>
        /// Length of time (in hours) of audio processed in included requests.
        /// </summary>
        [JsonProperty("total_hours")]
        public decimal TotalHours { get; set; }
        
        /// <summary>
        /// Number of included requests.
        /// </summary>
        [JsonProperty("requests")]
        public int Requests { get; set; }
    }
}
