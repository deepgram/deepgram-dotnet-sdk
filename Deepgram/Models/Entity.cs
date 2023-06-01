using System;
using Newtonsoft.Json;

namespace Deepgram.Models
{
    public class Entity
    {
        /// <summary>
        /// This is the type of the entity
        /// </summary>
        /// <remarks>e.g. DATE, PER, ORG, etc.</remarks>
        [JsonProperty("label")]
        public string Label { get; set; }

        /// <summary>
        /// This is the value of the detected entity.
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }

        /// <summary>
        /// Starting index of the entities words within the transcript.
        /// </summary>
        [JsonProperty("start_word")]
        public int StartWord { get; set; }

        /// <summary>
        /// Ending index of the entities words within the transcript.
        /// </summary>
        [JsonProperty("end_word")]
        public int EndWord { get; set; }

        /// <summary>
        /// Value between 0 and 1 indicating the model's relative confidence in this detected entity.
        /// </summary>
        [JsonProperty("confidence")]
        public decimal Confidence { get; set; }
    }
}
