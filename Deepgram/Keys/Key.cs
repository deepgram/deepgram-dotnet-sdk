using System;
using System.Text.Json.Serialization;

namespace Deepgram.Keys
{
    public class Key
    {
        /// <summary>
        /// Unique identifier of the Deepgram API key
        /// </summary>
        [JsonPropertyName("api_key_id")]
        public string Id { get; set; }

        /// <summary>
        /// Comment for the Deepgram API key
        /// </summary>
        [JsonPropertyName("comment")]
        public string Comment { get; set; }

        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        [JsonPropertyName("scopes")]
        public List<string> Scopes { get; set; }
    }
}
