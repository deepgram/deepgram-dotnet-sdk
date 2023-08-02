using Newtonsoft.Json;
using System;

namespace Deepgram.Models
{
    public class TopicList
    {
        /// <summary>
        /// Transcript covered by the topic.
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Word position in transcript where the topic begins
        /// </summary>
        [JsonProperty("start_word")]
        public int StartWord { get; set; }

        /// <summary>
        /// Word position in transcript where the topic ends
        /// </summary>
        [JsonProperty("end_word")]
        public int EndWord { get; set; }

        /// <summary>
        /// Array of Topics identified.
        /// </summary>
        [JsonProperty("topics")]
        public Topic[] Topics { get; set; }
    }
}
