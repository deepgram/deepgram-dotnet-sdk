using System;
using System.Text.Json.Serialization;

namespace Deepgram.Usage
{
    public class ListAllRequestsOptions
    {
        /// <summary>
        /// Start date of the requested date range.
        /// </summary>
        [JsonPropertyName("start")]
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// End date of the requested date range.
        /// </summary>
        [JsonPropertyName("end")]
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        /// Number of results to return per page. 
        /// </summary>
        /// <remarks>Defaults to 10</remarks>
        [JsonPropertyName("limit")]
        public int? Limit { get; set; }

        /// <summary>
        /// Status of requests to return.
        /// </summary>
        /// <remarks>Possible Values: null, succeeded OR failed</remarks>
        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }
}
