using Newtonsoft.Json;
using System;

namespace Deepgram.Models
{
    public class LiveTranscriptionMetaData
    {
        /// <summary>
        /// Unique identifier for the submitted audio and derived data returned.
        /// </summary>
        [JsonProperty("request_id")]
        public string Id { get; set; }

        /// <summary>
        /// Unique identifier for the submitted audio and derived data returned.
        /// </summary>
        [JsonProperty("model_uuid")]
        public string ModelUuid { get; set; }

        /// <summary>
        /// Info about the Model
        /// </summary>
        [JsonProperty("model_info")]
        public ModelInfo ModelInfo { get; set; }
    }
}
