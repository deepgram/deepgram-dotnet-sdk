using System;
using Newtonsoft.Json;

namespace Deepgram.Keys
{
    public class NewKey: Key
    {
        /// <summary>
        /// API Key
        /// </summary>
        /// <remarks>Only exists on this object. Subsequent requests to get keys will not have this property.</remarks>
        [JsonProperty("key")]
        public string Key { get; set; } = string.Empty;
    }
}
