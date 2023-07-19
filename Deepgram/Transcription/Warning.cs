using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Deepgram.Transcription
{
    public class Warning
    {
        /// <summary>
        /// Parameter sent in the request that resulted in the warning
        /// </summary>
        [JsonProperty("parameter")]
        public string Parameter { get; set; }

        /// <summary>
        /// The type of warning
        /// </summary>
        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public WarningType Type { get; set; }

        /// <summary>
        /// The warning message
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

    }
}


