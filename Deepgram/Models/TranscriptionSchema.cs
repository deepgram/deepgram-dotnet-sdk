namespace Deepgram.Models;
public class TranscriptionSchema
{
    /// <summary>
    /// AI model used to process submitted audio
    /// <see href="https://developers.deepgram.com/docs/model">
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    ///  Level of model you would like to use in your request.
    ///  <see href="https://developers.deepgram.com/docs/model">
    /// </summary>
    [JsonPropertyName("tier")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ModelTier? Tier { get; set; }

    /// <summary>
    /// Version of the model to use.
    /// <see href="https://developers.deepgram.com/docs/version">
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>
    /// Primary spoken language of submitted audio 
    /// <see href="https://developers.deepgram.com/docs/language">
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>
    /// Adds punctuation and capitalization to transcript
    /// <see href="https://developers.deepgram.com/docs/punctuation">
    /// </summary>
    [JsonPropertyName("punctuate")]
    public bool? Punctuate { get; set; }

    /// <summary>
    /// Profanity Filter looks for recognized profanity and converts it to the nearest recognized 
    /// non-profane word or removes it from the transcript completely.
    /// <see href="https://developers.deepgram.com/docs/profanity-filter">
    /// </summary>
    [JsonPropertyName("profanity_filter")]
    public bool? ProfanityFilter { get; set; }

    /// <summary>
    ///  Redaction removes sensitive information from your transcripts.
    ///  <see href="https://developers.deepgram.com/docs/redaction">
    /// </summary>
    [JsonPropertyName("redact")]
    public List<string>? Redact { get; set; }

    /// <summary>
    /// Diarize recognizes speaker changes and assigns a speaker to each word in the transcript. 
    /// <see href="https://developers.deepgram.com/docs/diarization">
    /// </summary>
    [JsonPropertyName("diarize")]
    public bool? Diarize { get; set; }

    /// <summary>
    /// Smart Format formats transcripts to improve readability. 
    /// <see href="https://developers.deepgram.com/docs/smart-format">
    /// </summary>
    [JsonPropertyName("smart_format")]
    public bool? SmartFormat { get; set; }

    /// <summary>
    /// Multichannel transcribes each channel in submitted audio independently. <see href="https://developers.deepgram.com/docs/multichannel">
    /// </summary>
    [JsonPropertyName("multichannel")]
    public bool? MultiChannel { get; set; }

    /// <summary>
    /// Multichannel transcribes each channel in submitted audio independently. 
    /// <see href="https://developers.deepgram.com/docs/">
    /// </summary>
    [JsonPropertyName("numerals")]
    public bool? Numerals { get; set; }

    /// <summary>
    /// Search searches for terms or phrases in submitted audio. 
    /// <see href="https://developers.deepgram.com/docs/search">
    /// </summary>
    [JsonPropertyName("search")]
    public string[]? Search { get; set; }

    /// <summary>
    /// Find and Replace searches for terms or phrases in submitted audio and replaces them.
    /// <see href="https://developers.deepgram.com/docs/find-and-replace">
    /// </summary>
    [JsonPropertyName("replace")]
    public string[]? Replace { get; set; }

    /// <summary>
    /// CallBack allows you to have your submitted audio processed asynchronously.
    /// <see href="https://developers.deepgram.com/docs/callback">
    /// </summary>
    [JsonPropertyName("callback")]

    public string? CallBack { get; set; }

    /// <summary>
    /// Keywords can boost or suppress specialized terminology.
    /// <see href="https://developers.deepgram.com/docs/keywords">
    /// </summary>
    [JsonPropertyName("keywords")]
    public string[]? Keywords { get; set; }

    /// <summary>
    /// Tagging allows you to label your requests for the purpose of identification during usage reporting.
    /// <see href="https://developers.deepgram.com/docs/tagging">
    /// </summary>
    [JsonPropertyName("tag")]
    public string[]? Tag { get; set; }

}
