using System;
using Newtonsoft.Json;

namespace Deepgram.Transcription
{
    public class ParagraphGroup
    {
        /// <summary>
        /// Full transcript
        /// </summary>
        [JsonProperty("transcript")]
        public string Transcript { get; set; } = string.Empty;

        /// <summary>
        /// Array of Paragraph objects.
        /// </summary>
        [JsonProperty("paragraphs")]
        public Paragraph[] Paragraphs { get; set; }
    }
}
