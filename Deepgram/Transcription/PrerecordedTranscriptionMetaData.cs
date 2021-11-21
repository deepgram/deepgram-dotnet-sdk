using System;
using Newtonsoft.Json;

namespace Deepgram.Transcription
{
    public class PrerecordedTranscriptionMetaData
    {
        /// <summary>
        /// Unique identifier for the submitted audio and derived data returned.
        /// </summary>
        [JsonProperty("request_id")]
        public string? Id { get; set; } = null;

        /// <summary>
        /// Blob of text that helps Deepgram engineers debug any problems you encounter.
        /// </summary>
        [JsonProperty("transaction_key")]
        public string? TransactionKey { get; set; } = null;

        /// <summary>
        /// SHA-256 hash of the submitted audio data.
        /// </summary>
        [JsonProperty("sha256")]
        public string? SHA256 { get; set; } = null;

        /// <summary>
        /// Timestamp that indicates when the audio was submitted.
        /// </summary>
        [JsonProperty("created")]
        public DateTime? Created { get; set; } = null;

        /// <summary>
        /// Duration in seconds of the submitted audio.
        /// </summary>
        [JsonProperty("duration")]
        public decimal? Duration { get; set; } = null;

        /// <summary>
        /// Number of channels detected in the submitted audio.
        /// </summary>
        [JsonProperty("channels")]
        public int? Channels { get; set; } = null;
    }
}
