using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Deepgram.Models
{


    public class CreateKeyOptions
    {
        /// <summary>
        /// Date on which the key you would like to create should expire.
        /// </summary>
        [JsonProperty("expiration_date")]
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Length of time (in seconds) during which the key you would like to create will remain valid.   
        /// </summary>
        [JsonProperty("time_to_live_in_seconds")]
        public int? TimeToLive { get; set; }

        /// <summary>
        ///   Tags associated with the key you would like to create
        /// </summary
        [JsonProperty("tags")]
        public string[]? Tags { get; set; }


    }
}
