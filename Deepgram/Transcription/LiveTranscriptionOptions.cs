using Newtonsoft.Json;

namespace Deepgram.Transcription
{
    public class LiveTranscriptionOptions
    {
        /// <summary>
        /// AI model used to process submitted audio
        /// </summary>
        [JsonProperty("model")]
        public string? Model { get; set; } = null;

        /// <summary>
        /// Version of the model to use.
        /// </summary>
        [JsonProperty("version")]
        public string? Version { get; set; } = null;

        /// <summary>
        /// BCP-47 language tag that hints at the primary spoken language.
        /// </summary>
        [JsonProperty("language")]
        public string? Language { get; set; } = null;

        /// <summary>
        /// Tier of model
        /// </summary>
        [JsonProperty("tier")]
        public string? Tier { get; set; } = null;

        /// <summary>
        /// Indicates whether to add punctuation and capitalization to the transcript.
        /// </summary>
        [JsonProperty("punctuate")]
        public bool? Punctuate { get; set; } = null;

        /// <summary>
        /// Indicates whether to remove profanity from the transcript.
        /// </summary>
        [JsonProperty("profanity_filter")]
        public bool? ProfanityFilter { get; set; } = null;

        /// <summary>
        /// Indicates whether to redact sensitive information, replacing redacted content with asterisks (*).
        /// </summary>
        /// <remarks>Possible values are: pci, numbers, ssn</remarks>
        [JsonProperty("redact")]
        public string[]? Redaction { get; set; }

        /// <summary>
        /// Indicates whether to recognize speaker changes. When set to true, each word in the transcript
        /// will be assigned a speaker number starting at 0. 
        /// </summary>
        [JsonProperty("diarize")]
        public bool? Diarize { get; set; } = null;

        /// <summary>
        /// Indicates whether to recognize alphanumeric strings. When set to true, whitespace will be removed
        /// between characters identified as part of an alphanumeric string. 
        /// </summary>
        [JsonProperty("ner")]
        public bool? NamedEntityRecognition { get; set; } = null;

        /// <summary>
        /// Indicates whether to transcribe each audio channel independently.
        /// </summary>
        [JsonProperty("multichannel")]
        public bool? MultiChannel { get; set; } = null;

        /// <summary>
        /// Maximum number of transcript alternatives to return.
        /// </summary>
        [JsonProperty("alternatives")]
        public int? Alternatives { get; set; } = null;

        /// <summary>
        /// Indicates whether to convert numbers from written format (e.g., one) to numerical format (e.g., 1).
        /// </summary>
        [JsonProperty("numerals")]
        public bool? Numerals { get; set; } = null;

        /// <summary>
        /// Terms or phrases to search for in the submitted audio.
        /// </summary>
        [JsonProperty("search")]
        public string[]? SearchTerms { get; set; }

        /// <summary>
        /// Callback URL to provide if you would like your submitted audio to be processed asynchronously.
        /// When passed, Deepgram will immediately respond with a request_id. 
        /// </summary>
        [JsonProperty("callback")]
        public string? Callback { get; set; } = null;

        /// <summary>
        /// Keywords to which the model should pay particular attention to boosting or suppressing to help
        /// it understand context.
        /// </summary>
        [JsonProperty("keywords")]
        public string[]? Keywords { get; set; }

        /// <summary>
        /// Indicates whether Deepgram will segment speech into meaningful semantic units, which allows
        /// the model to interact more naturally and effectively with speakers' spontaneous speech patterns.
        /// </summary>
        [JsonProperty("utterances")]
        public bool? Utterances { get; set; } = null;

        /// <summary>
        /// Indicates whether the streaming endpoint should send you updates to its transcription as more 
        /// audio becomes available.
        /// </summary>
        [JsonProperty("interim_results")]
        public bool? InterimResults { get; set; } = null;

        /// <summary>
        /// Indicates whether Deepgram will detect whether a speaker has finished speaking (or paused 
        /// for a significant period of time, indicating the completion of an idea).
        /// </summary>
        [JsonProperty("endpointing")]
        public bool? Endpointing { get; set; } = null;

        /// <summary>
        /// Length of time in milliseconds of silence that voice activation detection (VAD) will use 
        /// to detect that a speaker has finished speaking.
        /// </summary>
        [JsonProperty("vad_turnoff")]
        public int? VADTurnoff { get; set; } = null;

        /// <summary>
        /// Expected encoding of the submitted streaming audio.
        /// </summary>
        [JsonProperty("encoding")]
        public string? Encoding { get; set; } = null;

        /// <summary>
        /// Number of independent audio channels contained in submitted streaming audio.
        /// </summary>
        [JsonProperty("channels")]
        public int? Channels { get; set; } = null;

        /// <summary>
        /// Sample rate of submitted streaming audio. Required (and only read) when a value 
        /// is provided for encoding.
        /// </summary>
        [JsonProperty("sample_rate")]
        public int? SampleRate { get; set; } = null;
    }
}
