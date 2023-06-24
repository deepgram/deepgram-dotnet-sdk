using Newtonsoft.Json;
using System;

namespace Deepgram.Models
{
    public class UsageFields
    {
        /// <summary>
        /// Array of included tags.
        /// </summary>
        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        /// <summary>
        /// Array of included models.
        /// </summary>
        [JsonProperty("models")]
        public string[] Models { get; set; }

        /// <summary>
        /// Array of included processing methods.
        /// </summary>
        [JsonProperty("processing_methods")]
        public RequestMethod[] ProcessingMethods { get; set; }

        /// <summary>
        /// Array of included languages.
        /// </summary>
        [JsonProperty("languages")]
        public string[] Languages { get; set; }

        /// <summary>
        /// Array of included features.
        /// </summary>
        [JsonProperty("features")]
        public string[] Features { get; set; }
    }
}
