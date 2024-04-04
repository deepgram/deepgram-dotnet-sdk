// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.PreRecorded.v1;

public class PrerecordedSchema
{
    /// <summary>
    /// Number of transcripts to return per request
    /// <see href="https://developers.deepgram.com/reference/pre-recorded"/>
    /// Default is 1
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("alternatives")]
    public int? Alternatives { get; set; }

    /// <summary>
    /// CallBack allows you to have your submitted audio processed asynchronously.
    /// <see href="https://developers.deepgram.com/docs/callback">
    /// default is null
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("callback")]
    public string? CallBack { get; set; }

    /// <summary>
    /// Enables callback method
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("callback_method")]
    public bool? CallbackMethod { get; set; }

    /// <summary>
    /// Optional. A custom intent you want the model to detect within your input audio if present. Submit up to 100.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("custom_intent")]
    public List<string>? CustomIntent { get; set; }

    /// <summary>
    /// Optional. Sets how the model will interpret strings submitted to the custom_intent param. When "strict", the model will only return intents submitted using the custom_intent param. When "extended", the model will return it's own detected intents in addition those submitted using the custom_intents param.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("custom_intent_mode")]
    public string? CustomIntentMode { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("custom_topic")]
    public List<string>? CustomTopic { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("custom_topic_mode")]
    public string? CustomTopicMode { get; set; }

    /// <summary>
    /// Entity Detection identifies and extracts key entities from content in submitted audio
    /// <see href="https://developers.deepgram.com/docs/detect-entities">
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("detect_entities")]
    public bool? DetectEntities { get; set; }

    /// <summary>
    /// Language Detection identifies the dominant language spoken in submitted audio.
    /// <see href="https://developers.deepgram.com/docs/language-detection">
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("detect_language")]
    public bool? DetectLanguage { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("detect_topics")]
    public bool? DetectTopics { get; set; }

    /// <summary>
    /// Diarize recognizes speaker changes and assigns a speaker to each word in the transcript. 
    /// <see href="https://developers.deepgram.com/docs/diarization">
    /// default is false
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("diarize")]
    public bool? Diarize { get; set; }

    // <summary>
    /// <see href="https://developers.deepgram.com/docs/diarization">
    /// default is null, only applies if Diarize is set to true
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("diarize_version")]
    public string? DiarizeVersion { get; set; }

    // <summary>
    /// <see href="https://developers.deepgram.com/docs/diarization">
    /// default is null, only applies if Diarize is set to true
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("dictation")]
    public bool? Dictation { get; set; }

    /// <summary>
    /// Encoding allows you to specify the expected encoding of your submitted audio.
    /// <see href="https://developers.deepgram.com/docs/encoding">
    /// supported encodings <see cref="AudioEncoding"/>
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("encoding")]
    public string? Encoding { get; set; }

    /// <summary>
    /// Deepgram’s Extra Metadata feature allows you to attach arbitrary key-value pairs to your API requests that are attached to the API response for usage in downstream processing.
    /// Extra metadata is limited to 2048 characters per key-value pair.
    /// <see href="https://developers.deepgram.com/docs/extra-metadata"/>
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("extra")]
    public Dictionary<string, string>? Extra { get; set; }

    /// <summary>
    /// Whether to include words like "uh" and "um" in transcription output. 
    ///<see href="https://developers.deepgram.com/reference/pre-recorded"/>
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("filler_words")]
    public bool? FillerWords { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("intents")]
    public bool? Intents { get; set; }

    /// <summary>
    /// Keywords can boost or suppress specialized terminology.
    /// <see href="https://developers.deepgram.com/docs/keywords">
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("keywords")]
    public List<string>? Keywords { get; set; }

    /// <summary>
    /// Primary spoken language of submitted audio 
    /// <see href="https://developers.deepgram.com/docs/language">
    /// default value is 'en' 
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("measurements")]
    public bool? Measurements { get; set; }

    /// <summary>
    /// AI model used to process submitted audio
    /// <see href="https://developers.deepgram.com/docs/model">
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// Multichannel transcribes each channel in submitted audio independently. 
    /// <see href="https://developers.deepgram.com/docs/multichannel">
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("multichannel")]
    public bool? MultiChannel { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("paragraphs")]
    public bool? Paragraphs { get; set; }

    /// <summary>
    /// Profanity Filter looks for recognized profanity and converts it to the nearest recognized 
    /// non-profane word or removes it from the transcript completely.
    /// <see href="https://developers.deepgram.com/docs/profanity-filter">
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("profanity_filter")]
    public bool? ProfanityFilter { get; set; }

    /// <summary>
    /// Adds punctuation and capitalization to transcript
    /// <see href="https://developers.deepgram.com/docs/punctuation">
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("punctuate")]
    public bool? Punctuate { get; set; }

    /// <summary>
    ///  Indicates whether to redact sensitive information, replacing redacted content with asterisks (*). Can send multiple instances in query string (for example, redact=pci&redact=numbers).
    ///  <see href="https://developers.deepgram.com/docs/redaction">
    ///  default is List<string>("false") 
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("redact")]
    public List<string>? Redact { get; set; }

    /// <summary>
    /// Find and Replace searches for terms or phrases in submitted audio and replaces them.
    /// <see href="https://developers.deepgram.com/docs/find-and-replace">
    /// default is null
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("replace")]
    public List<string>? Replace { get; set; }

    /// <summary>
    /// Sample Rate allows you to specify the sample rate of your submitted audio.
    /// <see href="https://developers.deepgram.com/docs/sample-rate">
    /// only applies when Encoding has a value
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("sample_rate")]
    public int? SampleRate { get; set; }

    /// <summary>
    /// Search searches for terms or phrases in submitted audio. 
    /// <see href="https://developers.deepgram.com/docs/search">
    /// default is null
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("search")]
    public List<string>? Search { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("sentiment")]
    public bool? Sentiment { get; set; }

    /// <summary>
    /// Smart Format formats transcripts to improve readability. 
    /// <see href="https://developers.deepgram.com/docs/smart-format">
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("smart_format")]
    public bool? SmartFormat { get; set; }

    ///// <summary>
    ///// TODO
    ///// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("summarize")]
    public string? Summarize { get; set; }


    /// <summary>
    /// Tagging allows you to label your requests with one or more tags in a list,for the purpose of identification during usage reporting.
    /// <see href="https://developers.deepgram.com/docs/tagging">
    /// Default is a null 
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("tag")]
    public List<string>? Tag { get; set; }

    ///// <summary>
    ///// TODO
    ///// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("topics")]
    public bool? Topics { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("utt_split")]
    public double? UttSplit { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("utterances")]
    public bool? Utterances { get; set; }

    /// <summary>
    /// Version of the model to use.
    /// <see href="https://developers.deepgram.com/docs/version">
    /// default value is "latest"
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("version")]
    public string? Version { get; set; }
}
