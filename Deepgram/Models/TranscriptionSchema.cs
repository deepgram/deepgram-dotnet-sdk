namespace Deepgram.Models;
public class TranscriptionSchema
{
    /// <summary>
    /// Number of transcripts to return per request
    /// <see href="https://developers.deepgram.com/reference/pre-recorded"/>
    /// Default is 1
    /// </summary>
    [JsonPropertyName("alternatives")]
    public int Alternatives { get; set; } = 1;

    /// <summary>
    /// CallBack allows you to have your submitted audio processed asynchronously.
    /// <see href="https://developers.deepgram.com/docs/callback">
    /// default is null
    /// </summary>
    [JsonPropertyName("callback")]
    public string? CallBack { get; set; }

    /// <summary>
    /// Diarize recognizes speaker changes and assigns a speaker to each word in the transcript. 
    /// <see href="https://developers.deepgram.com/docs/diarization">
    /// default is false
    /// </summary>
    [JsonPropertyName("diarize")]
    public bool Diarize { get; set; } = default;

    // <summary>
    /// <see href="https://developers.deepgram.com/docs/diarization">
    /// default is null, only applies if Diarize is set to true
    /// </summary>
    [JsonPropertyName("diarize_version")]
    public string? DiarizeVersion { get; set; }

    /// <summary>
    /// Whether to include words like "uh" and "um" in transcription output. 
    ///<see href="https://developers.deepgram.com/reference/pre-recorded"/>
    /// </summary>
    [JsonPropertyName("filler_words")]
    public bool FillerWords { get; set; } = default;

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
    public string Language { get; set; } = "en";

    /// <summary>
    /// AI model used to process submitted audio
    /// <see href="https://developers.deepgram.com/docs/model">
    /// for possibles values <see cref="AIModel" /> default is General
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = "general";

    /// <summary>
    /// Multichannel transcribes each channel in submitted audio independently. <see href="https://developers.deepgram.com/docs/multichannel">
    /// </summary>
    [JsonPropertyName("multichannel")]
    public bool MultiChannel { get; set; } = default;

    /// <summary>
    /// Multichannel transcribes each channel in submitted audio independently. 
    /// <see href="https://developers.deepgram.com/docs/">
    /// available for English only
    /// </summary>
    [JsonPropertyName("numerals")]
    [Obsolete("Replaced with SmartFormat")]
    public bool? Numerals { get; set; }

    /// <summary>
    /// Profanity Filter looks for recognized profanity and converts it to the nearest recognized 
    /// non-profane word or removes it from the transcript completely.
    /// <see href="https://developers.deepgram.com/docs/profanity-filter">
    /// for use with base model tier only
    /// </summary>
    [JsonPropertyName("profanity_filter")]
    public bool ProfanityFilter { get; set; } = default;

    /// <summary>
    /// Adds punctuation and capitalization to transcript
    /// <see href="https://developers.deepgram.com/docs/punctuation">
    /// </summary>
    [JsonPropertyName("punctuate")]
    public bool Punctuate { get; set; } = default;

    /// <summary>
    ///  Indicates whether to redact sensitive information, replacing redacted content with asterisks (*). Can send multiple instances in query string (for example, redact=pci&redact=numbers).
    ///  <see href="https://developers.deepgram.com/docs/redaction">
    ///  default is List<string>("false") 
    /// </summary>
    [JsonPropertyName("redact")]
    public List<string> Redact { get; set; } = ["false"];

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

    /// <summary>
    /// Smart Format formats transcripts to improve readability. 
    /// <see href="https://developers.deepgram.com/docs/smart-format">
    /// </summary>
    [JsonPropertyName("smart_format")]
    public bool SmartFormat { get; set; } = default;

    /// <summary>
    /// Tagging allows you to label your requests with one or more tags in a list,for the purpose of identification during usage reporting.
    /// <see href="https://developers.deepgram.com/docs/tagging">
    /// Default is a null 
    /// </summary>
    [JsonPropertyName("tag")]
    public List<string>? Tag { get; set; }

    /// <summary>
    ///  Level of model you would like to use in your request.
    ///  <see href="https://developers.deepgram.com/docs/model">   
    /// </summary>
    [JsonPropertyName("tier")]
    [Obsolete("Use Model with joint model syntax https://developers.deepgram.com/docs/models-languages-overview")]
    public string? Tier { get; set; }

    /// <summary>
    /// Version of the model to use.
    /// <see href="https://developers.deepgram.com/docs/version">
    /// default value is "latest"
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = "latest";
}
