using System;
using Newtonsoft.Json;

namespace Deepgram.Usage
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
