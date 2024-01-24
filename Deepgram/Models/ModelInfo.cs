using System;
using Newtonsoft.Json;

namespace Deepgram.Models
{
    public class ModelInfo
    {
        /// <summary>
        /// Name of the Model
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Version of the Model
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; }

        /// <summary>
        /// Architecture of the Model
        /// </summary>
        [JsonProperty("arch")]
        public string Arch { get; set; }
    }
}
