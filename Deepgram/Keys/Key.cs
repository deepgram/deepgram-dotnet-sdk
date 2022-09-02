using System;
using Newtonsoft.Json;

namespace Deepgram.Keys
{
    public class Key
    {
        /// <summary>
        /// Unique identifier of the Deepgram API key
        /// </summary>
        [JsonProperty("api_key_id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Comment for the Deepgram API key
        /// </summary>
        [JsonProperty("comment")]
        public string Comment { get; set; } = string.Empty;

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("scopes")]
        public string[] Scopes { get; set; }
    }
}
