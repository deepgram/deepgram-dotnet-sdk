using System;
using Newtonsoft.Json;

namespace Deepgram.Transcription
{
    public class Sentence
    {
        /// <summary>
        /// Text transcript of the sentence.
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Offset in seconds from the start of the audio to where the sentence starts.
        /// </summary>
        [JsonProperty("start")]
        public decimal Start { get; set; }

        /// <summary>
        /// Offset in seconds from the start of the audio to where the sentence ends.
        /// </summary>
        [JsonProperty("end")]
        public decimal End { get; set; }
    }
}
