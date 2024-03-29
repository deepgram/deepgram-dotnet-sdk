using Newtonsoft.Json;
using System;

namespace Deepgram.Models
{
    public class LiveTranscriptionSpeechStarted
    {
        /// <summary>
        /// Timestamp since start of first recognition that speech started.
        /// </summary>
        [JsonProperty("timestamp")]
        public decimal Timestamp { get; set; }

        /// <summary>
        /// The channel field is interpreted as [A,B], where A is the channel index, and B is the total number of channels. The above example is channel 0 of single-channel audio.
        /// </summary>
        [JsonProperty("channel")]
        public int[] Channel { get; set; }

        /// <summary>
        /// The type field is always SpeechStarted for this event
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
