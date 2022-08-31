using System;
using Newtonsoft.Json;

namespace Deepgram.Transcription
{
    public class Alternative
    {
        /// <summary>
        /// Single-string transcript containing what the model hears in this channel of audio.
        /// </summary>
        [JsonProperty("transcript")]
        public string Transcript { get; set; } = string.Empty;

        /// <summary>
        /// Value between 0 and 1 indicating the model's relative confidence in this transcript.
        /// </summary>
        [JsonProperty("confidence")]
        public decimal Confidence { get; set; }

        /// <summary>
        /// Array of Word objects.
        /// </summary>
        [JsonProperty("words")]
        public Words[] Words { get; set; }
    }
}
