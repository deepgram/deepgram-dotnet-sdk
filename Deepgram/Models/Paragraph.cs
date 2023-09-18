using Newtonsoft.Json;
using System;

namespace Deepgram.Models
{
    public class Paragraph
    {
        /// <summary>
        /// Array of Sentence objects.
        /// </summary>
        [JsonProperty("sentences")]
        public Sentence[] Sentences { get; set; }

        /// <summary>
        /// Offset in seconds from the start of the audio to where the paragraph starts.
        /// </summary>
        [JsonProperty("start")]
        public decimal Start { get; set; }

        /// <summary>
        /// Offset in seconds from the start of the audio to where the paragraph ends.
        /// </summary>
        [JsonProperty("end")]
        public decimal End { get; set; }

        /// <summary>
        /// Number of words in the paragraph
        /// </summary>
        [JsonProperty("num_words")]
        public int NumberOfWords { get; set; }

        /// <summary>
        /// Speaker of the paragraph
        /// </summary>
        [JsonProperty("speaker")]
        public string Speaker { get; set; }
    }
}
