using Newtonsoft.Json;
using System;

namespace Deepgram.Models
{
    public class PrerecordedTranscriptionMetaData
    {
        /// <summary>
        /// Unique identifier for the submitted audio and derived data returned.
        /// </summary>
        [JsonProperty("request_id")]
        public string Id { get; set; }

        /// <summary>
        /// Blob of text that helps Deepgram engineers debug any problems you encounter.
        /// </summary>
        [JsonProperty("transaction_key")]
        public string TransactionKey { get; set; }

        /// <summary>
        /// SHA-256 hash of the submitted audio data.
        /// </summary>
        [JsonProperty("sha256")]
        public string SHA256 { get; set; }

        /// <summary>
        /// Timestamp that indicates when the audio was submitted.
        /// </summary>
        [JsonProperty("created")]
        public DateTime Created { get; set; }

        /// <summary>
        /// Duration in seconds of the submitted audio.
        /// </summary>
        [JsonProperty("duration")]
        public decimal Duration { get; set; }

        /// <summary>
        /// Number of channels detected in the submitted audio.
        /// </summary>
        [JsonProperty("channels")]
        public int Channels { get; set; }

        /// <summary>
        /// Warnings to provide feedback about unsupported and deprecated queries.
        /// </summary>
        [JsonProperty("warnings")]
        public Warning[] Warnings  { get; set; }

        /// <summary>
        /// Models used in this API Request
        /// </summary>
        [JsonProperty("models")]
        public Guid[] Models { get; set; }

        /// <summary>
        /// Info about the Model
        /// </summary>
        [JsonProperty("model_info")]
        public ModelInfo ModelInfo { get; set; }

        /// <summary>
        /// Allows labeling your requests for the purpose of identification during usage reporting.
        /// </summary>
        [JsonProperty("tags")]
        public string[] Tags { get; set; }
    }
}
