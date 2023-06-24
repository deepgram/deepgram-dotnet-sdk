using Newtonsoft.Json;
using System;

namespace Deepgram.Models
{
    public class Hit
    {
        /// <summary>
        /// Value between 0 and 1 that indicates the model's relative confidence in this hit.
        /// </summary>
        [JsonProperty("confidence")]
        public decimal Confidence { get; set; }

        /// <summary>
        /// Offset in seconds from the start of the audio to where the hit occurs.
        /// </summary>
        [JsonProperty("start")]
        public decimal Start { get; set; }

        /// <summary>
        /// Offset in seconds from the start of the audio to where the hit ends.
        /// </summary>
        [JsonProperty("end")]
        public decimal End { get; set; }

        /// <summary>
        /// Transcript that corresponds to the time between start and end.
        /// </summary>
        [JsonProperty("snippet")]
        public string Snippet { get; set; }
    }
}
