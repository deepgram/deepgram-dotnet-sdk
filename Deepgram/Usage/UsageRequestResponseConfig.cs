using System;
using Newtonsoft.Json;

namespace Deepgram.Usage
{
    public class UsageRequestResponseConfig
    {
        /// <summary>
        /// Was the diarization featured used?
        /// </summary>
        [JsonProperty("diarize")]
        public bool? Diarize { get; set; }

        /// <summary>
        /// Was the multichannel feature used?
        /// </summary>
        [JsonProperty("multichannel")]
        public bool? MultiChannel { get; set; }
    }
}
