using System;
using System.Text.Json.Serialization;

namespace Deepgram.Usage
{
    public class UsageRequestResponseConfig
    {
        /// <summary>
        /// Was the diarization featured used?
        /// </summary>
        [JsonPropertyName("diarize")]
        public bool? Diarize { get; set; }

        /// <summary>
        /// Was the multichannel feature used?
        /// </summary>
        [JsonPropertyName("multichannel")]
        public bool? MultiChannel { get; set; }
    }
}
