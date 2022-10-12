using System;
using Newtonsoft.Json;

namespace Deepgram.Usage
{
    public class UsageRequestResponse
    {
        /// <summary>
        /// Details of the request
        /// </summary>
        [JsonProperty("details")]
        public UsageRequestResponseDetail Details {get;set;}

        /// <summary>
        /// Status Code of the response
        /// </summary>
        [JsonProperty("code")]
        public int? Code { get; set; }

        /// <summary>
        /// DateTime the response completed
        /// </summary>
        [JsonProperty("completed")]
        public DateTime? Completed { get; set; }
    }
}
