using Newtonsoft.Json;
using System;

namespace Deepgram.Models
{
    public class ApiKey
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

        [JsonProperty("tags")]
        public string[] Tags { get; set; } = Array.Empty<string>();

        [JsonProperty("expiration_date")]
        public DateTime? ExpirationDate { get; set; }
    }
}
