using System;
using Newtonsoft.Json;

namespace Deepgram.Usage
{
    public class ListAllRequestsResponse
    {
        /// <summary>
        /// Number of results to return per page. 
        /// </summary>
        [JsonProperty("limit")]
        public int Limit { get; set; }

        /// <summary>
        /// Page number that should be returned.
        /// </summary>
        [JsonProperty("page")]
        public int Page { get; set; }

        /// <summary>
        /// Array of requests
        /// </summary>
        [JsonProperty("requests")]
        public UsageRequest[] Requests { get; set; }
    }
}
