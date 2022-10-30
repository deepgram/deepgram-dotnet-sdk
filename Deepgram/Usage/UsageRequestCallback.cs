using System;
using Newtonsoft.Json;

namespace Deepgram.Usage
{
    /// <summary>
    ///  If a callback was included in the request, but the request has not completed yet, 
    ///  then this object will exist, but it will be empty.
    /// </summary>
    public class UsageRequestCallback
    {
        /// <summary>
        /// Number of attempts of the callback
        /// </summary>
        [JsonProperty("attempts")]
        public int? attempts { get; set; }

        /// <summary>
        /// Status Code of the callback
        /// </summary>
        [JsonProperty("code")]
        public int? Code { get; set; }

        /// <summary>
        /// DateTime the callback completed
        /// </summary>
        [JsonProperty("completed")]
        public DateTime? Completed { get; set; }
    }
}
