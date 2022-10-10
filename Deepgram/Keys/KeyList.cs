using Newtonsoft.Json;

namespace Deepgram.Keys
{
    public class KeyList
    {
        /// <summary>
        /// List of Deepgram api keys
        /// </summary>
        [JsonProperty("api_keys")]
        public Key[] ApiKeys { get; set; }
    }
}
