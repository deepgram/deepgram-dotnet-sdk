using System;
using System.Text.Json.Serialization;

namespace Deepgram.Keys
{
    public class NewKey: Key
    {
        /// <summary>
        /// API Key
        /// </summary>
        /// <remarks>Only exists on this object. Subsequent requests to get keys will not have this property.</remarks>
        [JsonPropertyName("key")]
        public string Key { get; set; }
    }
}
