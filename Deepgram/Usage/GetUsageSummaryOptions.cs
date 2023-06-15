using System;
using Newtonsoft.Json;

namespace Deepgram.Usage
{
    public class GetUsageSummaryOptions
    {
        /// <summary>
        /// Start date of the requested date range.
        /// </summary>
        [JsonProperty("start")]
        public Nullable<DateTime> StartDateTime { get; set; } = null;

        /// <summary>
        /// End date of the requested date range.
        /// </summary>
        [JsonProperty("end")]
        public Nullable<DateTime> EndDateTime { get; set; } = null;

        /// <summary>
        /// Limits results to requests made using the API key corresponding to the given accessor. 
        /// </summary>
        [JsonProperty("accessor")]
        public string ApiKeyId { get; set; } = null;

        /// <summary>
        /// Limits results to requests associated with the specified tag(s). 
        /// </summary>
        [JsonProperty("tag")]
        public string[] Tag { get; set; }

        /// <summary>
        /// Limits results to requests processed using the specified method.
        /// </summary>
        [JsonProperty("method")]
        public Nullable<RequestMethod> Method { get; set; } = null;

        /// <summary>
        /// Limits results to requests run with the specified model applied.
        /// </summary>
        [JsonProperty("model")]
        public string[] Model { get; set; } = null;

        /// <summary>
        /// Limits results to requests that include the multichannel feature.
        /// </summary>
        [JsonProperty("multichannel")]
        public Nullable<bool> MultiChannel { get; set; } = null;

        /// <summary>
        /// Limits results to requests that include the interim_results feature.
        /// </summary>
        [JsonProperty("interim_results")]
        public Nullable<bool> InterimResults { get; set; } = null;

        /// <summary>
        /// Limits results to requests that include the punctuate feature.
        /// </summary>
        [JsonProperty("punctuate")]
        public Nullable<bool> Punctuate { get; set; } = null;

        /// <summary>
        /// Limits results to requests that include the utterances feature.
        /// </summary>
        [JsonProperty("utterances")]
        public Nullable<bool> Utterances { get; set; } = null;

        /// <summary>
        /// Limits results to requests that include the replace feature.
        /// </summary>
        [JsonProperty("replace")]
        public Nullable<bool> Replace { get; set; } = null;

        /// <summary>
        /// Limits results to requests that include the profanity_filter feature.
        /// </summary>
        [JsonProperty("profanity_filter")]
        public Nullable<bool> ProfanityFilter { get; set; } = null;

        /// <summary>
        /// Limits results to requests that include the keywords feature.
        /// </summary>
        [JsonProperty("keywords")]
        public Nullable<bool> Keywords { get; set; } = null;

        /// <summary>
        /// Limits results to requests that include the sentiment feature.
        /// </summary>
        [JsonProperty("analyze_sentiment")]
        public Nullable<bool> Sentiment { get; set; } = null;

        /// <summary>
        /// Limits results to requests that include the sentiment threshold feature.
        /// </summary>
        [JsonProperty("sentiment_threshold")]
        public Nullable<bool> SentimentThreshold { get; set; } = null;

        /// <summary>
        /// Limits results to requests that include the diarize feature.
        /// </summary>
        [JsonProperty("diarize")]
        public Nullable<bool> Diarization { get; set; } = null;

        /// <summary>
        /// Limits results to requests that include the detect_language feature.
        /// </summary>
        [JsonProperty("detect_language")]
        public Nullable<bool> DetectLanguage { get; set; } = null;

        /// <summary>
        /// Limits results to requests that include the search feature.
        /// </summary>
        [JsonProperty("search")]
        public Nullable<bool> Search { get; set; } = null;

        /// <summary>
        /// Limits results to requests that include the redact feature.
        /// </summary>
        [JsonProperty("redact")]
        public Nullable<bool> Redaction { get; set; } = null;

        /// <summary>
        /// Limits results to requests that include the alternatives feature.
        /// </summary>
        [JsonProperty("alternatives")]
        public Nullable<bool> Alternatives { get; set; } = null;

        /// <summary>
        /// Limits results to requests that include the numerals feature.
        /// </summary>
        [JsonProperty("numerals")]
        public Nullable<bool> Numerals { get; set; } = null;

        /// <summary>
        /// Limits results to requests that include the paragraphs feature.
        /// </summary>
        [JsonProperty("paragraphs")]
        public Nullable<bool> Paragraphs { get; set; } = null;

        /// <summary>
        /// Limits results to requests that include the detect entities feature.
        /// </summary>
        [JsonProperty("detect_entities")]
        public Nullable<bool> DetectEntities { get; set; } = null;

        /// <summary>
        /// Limits results to requests that include the summarize feature.
        /// </summary>
        [JsonProperty("summarize")]
        public Nullable<bool> Summarize { get; set; } = null;

        /// <summary>
        /// Limits results to requests that include the translation feature.
        /// </summary>
        [JsonProperty("translate")]
        public Nullable<bool> Translate { get; set; } = null;

        /// <summary>
        /// Limits results to requests that include the topic detection feature.
        /// </summary>
        [JsonProperty("detect_topics")]
        public Nullable<bool> DetectTopics { get; set; } = null;
    }
}
