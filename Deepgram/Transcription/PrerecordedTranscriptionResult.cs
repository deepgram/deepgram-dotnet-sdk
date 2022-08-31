using System;
using Newtonsoft.Json;

namespace Deepgram.Transcription
{
    public class PrerecordedTranscriptionResult
    {
        /// <summary>
        /// Array of Channel objects.
        /// </summary>
        [JsonProperty("channels")]
        public Channel[] Channels { get; set; }

        /// <summary>
        /// Array of Utterance objects. 
        /// </summary>
        [JsonProperty("utterances")]
        public Utterance[] Utterances { get; set; }
    }
}
