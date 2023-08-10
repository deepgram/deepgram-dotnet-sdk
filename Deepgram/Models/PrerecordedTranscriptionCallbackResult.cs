using System;
using Newtonsoft.Json;

namespace Deepgram.Models
{
    public class PrerecordedTranscriptionCallbackResult
    {
        /// <summary>
        /// Id of the request.
        /// </summary>
        [JsonProperty("request_id")]
        public Guid RequestId { get; set; }
    }
}
