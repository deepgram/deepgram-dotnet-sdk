using System;
using System.Text.Json.Serialization;

namespace Deepgram.Usage
{
    public class UsageSummaryResolution
    {
        /// <summary>
        /// Units of resolution amount.
        /// </summary>
        [JsonPropertyName("units")]
        public string Units { get; set; }

        /// <summary>
        /// Number of days
        /// </summary>
        [JsonPropertyName("amount")]
        public int Amount { get; set; }
    }
}
