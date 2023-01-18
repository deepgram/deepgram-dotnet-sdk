using Newtonsoft.Json;

namespace Deepgram.Transcription
{
    public class PrerecordedTranscriptionRequest
    {
        /// <summary>
        /// Unique identifier for the submitted audio and derived data returned.
        /// </summary>
        [JsonProperty("request_id")]
        public string Id { get; set; }
    }
}
