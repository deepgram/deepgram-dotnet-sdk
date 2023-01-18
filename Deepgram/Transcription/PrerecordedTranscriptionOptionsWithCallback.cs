using Newtonsoft.Json;

namespace Deepgram.Transcription
{
    public class PrerecordedTranscriptionOptionsWithCallback : PrerecordedTranscriptionOptions
    {
        /// <summary>
        /// Callback URL to provide if you would like your submitted audio to be processed asynchronously.
        /// When passed, Deepgram will immediately respond with a request_id. 
        /// </summary>
        [JsonProperty("callback")]
        public string Callback { get; set; } = null;
    }
}
