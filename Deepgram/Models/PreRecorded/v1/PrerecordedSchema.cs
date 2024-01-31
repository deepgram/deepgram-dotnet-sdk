namespace Deepgram.Models.PreRecorded.v1;

public class PrerecordedSchema
{

    /// <summary>
    /// Number of transcripts to return per request
    /// <see href="https://developers.deepgram.com/reference/pre-recorded"/>
    /// Default is 1
    /// </summary>
    [JsonPropertyName("alternatives")]
    public int? Alternatives { get; set; }

    /// <summary>
    /// CallBack allows you to have your submitted audio processed asynchronously.
    /// <see href="https://developers.deepgram.com/docs/callback">
    /// default is null
    /// </summary>
    [JsonPropertyName("callback")]
    public string? CallBack { get; set; }

    /// <summary>
    /// Enables callback method
    /// </summary>
    [JsonPropertyName("callback_method")]
    public bool? CallbackMethod { get; set; }

    ///// <summary>
    ///// Optional. A custom intent you want the model to detect within your input audio if present. Submit up to 100.
    ///// </summary>
    //[JsonPropertyName("custom_intent")]
    //public string CustomIntent { get; set; }

    ///// <summary>
    ///// Optional. Sets how the model will interpret strings submitted to the custom_intent param. When "strict", the model will only return intents submitted using the custom_intent param. When "extended", the model will return it's own detected intents in addition those submitted using the custom_intents param.
    ///// </summary>
    //[JsonPropertyName("custom_intent_mode")]
    //public string CustomIntentMode { get; set; }


    /// <summary>
    /// Diarize recognizes speaker changes and assigns a speaker to each word in the transcript. 
    /// <see href="https://developers.deepgram.com/docs/diarization">
    /// default is false
    /// </summary>
    [JsonPropertyName("diarize")]
    public bool? Diarize { get; set; }

    // <summary>
    /// <see href="https://developers.deepgram.com/docs/diarization">
    /// default is null, only applies if Diarize is set to true
    /// </summary>
    [JsonPropertyName("diarize_version")]
    public string? DiarizeVersion { get; set; }

    /// <summary>
    /// Deepgram’s Extra Metadata feature allows you to attach arbitrary key-value pairs to your API requests that are attached to the API response for usage in downstream processing.
    /// Extra metadata is limited to 2048 characters per key-value pair.
    /// <see href="https://developers.deepgram.com/docs/extra-metadata"/>
    /// </summary>
    [JsonPropertyName("extra")]
    public Dictionary<string, string> Extra { get; set; }


    /// <summary>
    /// Whether to include words like "uh" and "um" in transcription output. 
    ///<see href="https://developers.deepgram.com/reference/pre-recorded"/>
    /// </summary>
    [JsonPropertyName("filler_words")]
    public bool? FillerWords { get; set; }

    ///// <summary>
    ///// Enables intent recognition
    ///// </summary>
    //[JsonPropertyName("intents")]
    //public bool? Intents { get; set; }

    /// <summary>
    /// Keywords can boost or suppress specialized terminology.
    /// <see href="https://developers.deepgram.com/docs/keywords">
    /// </summary>
    [JsonPropertyName("keywords")]
    public List<string>? Keywords { get; set; }

    /// <summary>
    /// Primary spoken language of submitted audio 
    /// <see href="https://developers.deepgram.com/docs/language">
    /// default value is 'en' 
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>
    /// AI model used to process submitted audio
    /// <see href="https://developers.deepgram.com/docs/model">
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// Multichannel transcribes each channel in submitted audio independently. 
    /// <see href="https://developers.deepgram.com/docs/multichannel">
    /// </summary>
    [JsonPropertyName("multichannel")]
    public bool? MultiChannel { get; set; }


    /// <summary>
    /// Profanity Filter looks for recognized profanity and converts it to the nearest recognized 
    /// non-profane word or removes it from the transcript completely.
    /// <see href="https://developers.deepgram.com/docs/profanity-filter">
    /// for use with base model tier only
    /// </summary>
    [JsonPropertyName("profanity_filter")]
    public bool? ProfanityFilter { get; set; }

    /// <summary>
    /// Adds punctuation and capitalization to transcript
    /// <see href="https://developers.deepgram.com/docs/punctuation">
    /// </summary>
    [JsonPropertyName("punctuate")]
    public bool? Punctuate { get; set; }

    /// <summary>
    ///  Indicates whether to redact sensitive information, replacing redacted content with asterisks (*). Can send multiple instances in query string (for example, redact=pci&redact=numbers).
    ///  <see href="https://developers.deepgram.com/docs/redaction">
    ///  default is List<string>("false") 
    /// </summary>
    [JsonPropertyName("redact")]
    public List<string>? Redact { get; set; }

    /// <summary>
    /// Find and Replace searches for terms or phrases in submitted audio and replaces them.
    /// <see href="https://developers.deepgram.com/docs/find-and-replace">
    /// default is null
    /// </summary>
    [JsonPropertyName("replace")]
    public List<string>? Replace { get; set; }

    /// <summary>
    /// Search searches for terms or phrases in submitted audio. 
    /// <see href="https://developers.deepgram.com/docs/search">
    /// default is null
    /// </summary>
    [JsonPropertyName("search")]
    public List<string>? Search { get; set; }

    ///// <summary>
    ///// Enables sentiment analysis false by default
    ///// </summary>
    //[JsonPropertyName("sentiment")]
    //public bool? Sentiment { get; set; }

    //[JsonPropertyName("sentiment_threshold")]
    //public double? SentimentThreshold { get; set; }

    /// <summary>
    /// Smart Format formats transcripts to improve readability. 
    /// <see href="https://developers.deepgram.com/docs/smart-format">
    /// </summary>
    [JsonPropertyName("smart_format")]
    public bool? SmartFormat { get; set; }

    /// <summary>
    /// Tagging allows you to label your requests with one or more tags in a list,for the purpose of identification during usage reporting.
    /// <see href="https://developers.deepgram.com/docs/tagging">
    /// Default is a null 
    /// </summary>
    [JsonPropertyName("tag")]
    public List<string>? Tag { get; set; }

    ///// <summary>
    /////  Level of model you would like to use in your request.
    /////  <see href="https://developers.deepgram.com/docs/model">   
    ///// </summary>
    //[JsonPropertyName("tier")]
    //[Obsolete("Use Model with joint model syntax https://developers.deepgram.com/docs/models-languages-overview")]
    //public string? Tier { get; set; }

    /// <summary>
    /// Version of the model to use.
    /// <see href="https://developers.deepgram.com/docs/version">
    /// default value is "latest"
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>
    /// Entity Detection identifies and extracts key entities from content in submitted audio
    /// <see href="https://developers.deepgram.com/docs/detect-entities">
    /// </summary>
    [JsonPropertyName("detect_entities")]
    public bool? DetectEntities { get; set; }

    /// <summary>
    /// Language Detection identifies the dominant language spoken in submitted audio.
    /// <see href="https://developers.deepgram.com/docs/language-detection">
    /// </summary>
    [JsonPropertyName("detect_language")]
    public bool? DetectLanguage { get; set; }

    /// <summary>
    /// Topic Detection identifies and extracts key topics from content in submitted audio. 
    /// <see href="https://developers.deepgram.com/docs/topic-detection">
    /// Default is false
    /// </summary>
    [JsonPropertyName("detect_topics")]
    public bool? DetectTopics { get; set; }

    /// <summary>
    /// Spoken dictation commands will be converted to their corresponding punctuation marks. e.g., comma to ,
    /// <see href="https://developers.deepgram.com/reference/pre-recorded"/>
    /// Default is false
    /// </summary>
    [JsonPropertyName("dictation")]
    public bool? Dictation { get; set; }

    /// <summary>
    /// Spoken measurements will be converted to their corresponding abbreviations. e.g., milligram to mg
    /// Default is false
    /// </summary>
    [JsonPropertyName("measurements")]
    public bool? Measurements { get; set; }

    [JsonPropertyName("ner")]
    [Obsolete("Replaced with SmartFormat")]
    public bool? Ner { get; set; }

    /// <summary>
    /// Paragraphs splits audio into paragraphs to improve transcript readability.
    /// <see href="https://developers.deepgram.com/docs/paragraphs">
    /// </summary>
    [JsonPropertyName("paragraphs")]
    public bool? Paragraphs { get; set; }

    /// <summary>
    /// Summarizes content of submitted audio. 
    /// <see href="https://developers.deepgram.com/docs/summarization">
    /// Default is v2
    /// </summary>
    [JsonPropertyName("summarize")]
    public object? Summarize { get; set; }

    /// <summary>
    /// Utterances segments speech into meaningful semantic units.
    /// <see href="https://developers.deepgram.com/docs/utterances">
    /// default is false
    /// </summary>
    [JsonPropertyName("utterances")]
    public bool? Utterances { get; set; }

    /// <summary>
    /// Utterance Split detects pauses between words in submitted audio. 
    /// Used when the Utterances feature is enabled for pre-recorded audio.
    /// <see href="https://developers.deepgram.com/docs/utterance-split">
    /// Default is 0.8
    /// </summary>
    [JsonPropertyName("utt_split")]
    public double? UttSplit { get; set; }

}
