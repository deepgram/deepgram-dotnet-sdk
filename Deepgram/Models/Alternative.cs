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

        /// <summary>
        /// <see cref="ParagraphGroup"/> containing /n seperated transcript and <see cref="Paragraph"/> objects.
        /// </summary>
        /// <remark>Only used when the paragraph feature is enabled on the request</remark>
        [JsonProperty("paragraphs")]
        public ParagraphGroup Paragraphs { get; set; }

        /// <summary>
        /// Array of Summary objects.
        /// </summary>
        /// <remark>Only used when the summarize feature is enabled on the request</remark>
        [JsonProperty("summaries")]
        public Summary[] Summaries { get; set; }

        /// <summary>
        /// Array of Entity objects.
        /// </summary>
        /// <remark>Only used when the detect entities feature is enabled on the request</remark>
        [JsonProperty("entities")]
        public Entity[] Entities { get; set; }

        /// <summary>
        /// Array of Translation objects.
        /// </summary>
        /// <remark>Only used when the translation feature is enabled on the request</remark>
        [JsonProperty("translations")]
        public Translation[] Translations { get; set; }
    }
}
