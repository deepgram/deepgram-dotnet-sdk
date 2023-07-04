using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Deepgram.Keys

{
    public class CreateKeyOptions
    {
        /// <summary>
        /// Date on which the key you would like to create should expire.
        /// </summary>
        [JsonProperty("expiration_date")]
        public Nullable<DateTime> ExpirationDate { get; set; } = null;

        /// <summary>
        /// Length of time (in seconds) during which the key you would like to create will remain valid.   
        /// </summary>
        [JsonProperty("time_to_live_in_seconds")]
        public Nullable<int> TimeToLive { get; set; } = null;

        /// <summary>
        ///   Tags associated with the key you would like to create
        /// </summary
        [JsonProperty("tags")]
        public string[] Tags { get; set; } = null;


    }
}
