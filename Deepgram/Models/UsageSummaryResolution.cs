using Newtonsoft.Json;
using System;

namespace Deepgram.Models
{
    public class UsageSummaryResolution
    {
        /// <summary>
        /// Units of resolution amount.
        /// </summary>
        [JsonProperty("units")]
        public string Units { get; set; } = string.Empty;

        /// <summary>
        /// Number of days
        /// </summary>
        [JsonProperty("amount")]
        public int Amount { get; set; }
    }
}
