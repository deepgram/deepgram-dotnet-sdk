using System;
using System.Text.Json.Serialization;

namespace Deepgram.Usage
{
    public class UsageFields
    {
        /// <summary>
        /// Array of included tags.
        /// </summary>
        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }

        /// <summary>
        /// Array of included models.
        /// </summary>
        [JsonPropertyName("models")]
        public List<string> Models { get; set; }

        /// <summary>
        /// Array of included processing methods.
        /// </summary>
        [JsonPropertyName("processing_methods")]
        public List<RequestMethod> ProcessingMethods { get; set; }

        /// <summary>
        /// Array of included features.
        /// </summary>
        [JsonPropertyName("features")]
        public List<string> Features { get; set; }
    }
}
