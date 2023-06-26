using Newtonsoft.Json;
using System;

namespace Deepgram.Models
{
    public class UsageRequestResponseConfig
    {
        /// <summary>
        /// Requested maximum number of transcript alternatives to return.
        /// </summary>
        [JsonProperty("alternatives")]
        public int? Alternatives { get; set; } = null;

        /// <summary>
        /// Indicates whether diarization was requested.
        /// </summary>
        [JsonProperty("diarize")]
        public bool? Diarize { get; set; } = null;

        /// <summary>
        /// Indicates whether multichannel processing was requested.
        /// </summary>
        [JsonProperty("multichannel")]
        public bool? MultiChannel { get; set; } = null;

        /// <summary>
        /// Indicates whether detect language feature was requested.
        /// </summary>
        [JsonProperty("detect_language")]
        public bool? DetectLanguage { get; set; } = null;

        /// <summary>
        /// Indicates whether paragraphs feature was requested.
        /// </summary>
        [JsonProperty("paragraphs")]
        public bool? Paragraphs { get; set; } = null;

        /// <summary>
        /// Indicates whether detect entities feature was requested.
        /// </summary>
        [JsonProperty("detect_entities")]
        public bool? DetectEntities { get; set; } = null;

        /// <summary>
        /// Indicates whether summarize v1 feature was requested.
        /// </summary>
        [JsonProperty("summarize")]
        public bool? Summarize { get; set; } = null;

        /// <summary>
        /// Indicates whether summarize v2 feature was requested.
        /// </summary>
        [JsonProperty("summarize")]
        public string? Summarize { get; set; } = null;

        /// <summary>
        /// Array of keywords associated with the request.
        /// </summary>
        [JsonProperty("keywords")]
        public string[] Keywords { get; set; } = null;

        /// <summary>
        /// Language associated with the request.
        /// </summary>
        [JsonProperty("language")]
        public string Language { get; set; } = null;

        /// <summary>
        /// Model associated with the request.
        /// </summary>
        [JsonProperty("model")]
        public string Model { get; set; } = null;

        /// <summary>
        /// Indicates whether named-entity recognition (NER) was requested.
        /// </summary>
        [JsonProperty("ner")]
        public bool? NamedEntityRecognition { get; set; } = null;

        /// <summary>
        /// Indicates whether numeral conversion was requested.
        /// </summary>
        [JsonProperty("numerals")]
        public bool? Numerals { get; set; } = null;

        /// <summary>
        /// Indicates whether filtering profanity was requested.
        /// </summary>
        [JsonProperty("profanity_filter")]
        public bool? ProfanityFilter { get; set; } = null;

        /// <summary>
        /// Indicates whether punctuation was requested.
        /// </summary>
        [JsonProperty("punctuate")]
        public bool? Punctuate { get; set; } = null;

        /// <summary>
        /// Indicates whether redaction was requested.
        /// </summary>
        [JsonProperty("redact")]
        public string[] Redaction { get; set; } = null;

        /// <summary>
        /// Array of seach terms associated with the request.
        /// </summary>
        [JsonProperty("search")]
        public string[] Search { get; set; } = null;

        /// <summary>
        /// Indicates whether utterance segmentation was requested.
        /// </summary>
        [JsonProperty("utterances")]
        public bool? Utterances { get; set; } = null;

        /// <summary>
        /// Array of translations associated with the request.
        /// </summary>
        [JsonProperty("translation")]
        public string[] Translation { get; set; } = null;

        /// <summary>
        /// Indicates whether topic detection was requested.
        /// </summary>
        [JsonProperty("detect_topics")]
        public bool? DetectTopics { get; set; } = null;
    }
}
