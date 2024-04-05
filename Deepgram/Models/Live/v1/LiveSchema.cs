// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Live.v1;

public class LiveSchema
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
    /// Channels allows you to specify the number of independent audio channels your submitted audio contains. 
    /// Used when the Encoding feature is also being used to submit streaming raw audio
    /// <see href="https://developers.deepgram.com/docs/channels">
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("channels")]
    public int? Channels { get; set; }

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

    /// <summary>
    /// Encoding allows you to specify the expected encoding of your submitted audio.
    /// <see href="https://developers.deepgram.com/docs/encoding">
    /// supported encodings <see cref="AudioEncoding"/>
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("encoding")]
    public string? Encoding { get; set; }

    /// <summary>
    /// Endpointing returns transcripts when pauses in speech are detected.
    /// <see href="https://developers.deepgram.com/docs/endpointing">
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("endpointing")]
    public string? EndPointing { get; set; }

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
    /// Interim Results provides preliminary results for streaming audio to solve the need for immediate results combined with high levels of accuracy.
    /// <see href="https://developers.deepgram.com/docs/interim-results">
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("interim_results")]
    public bool? InterimResults { get; set; }

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
    /// Profanity Filter looks for recognized profanity and converts it to the nearest recognized 
    /// non-profane word or removes it from the transcript completely.
    /// <see href="https://developers.deepgram.com/docs/profanity-filter">
    /// for use with base model tier only
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
    /// Smart Format formats transcripts to improve readability. 
    /// <see href="https://developers.deepgram.com/docs/smart-format">
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("smart_format")]
    public bool? SmartFormat { get; set; }

    /// <summary>
    /// Tagging allows you to label your requests with one or more tags in a list,for the purpose of identification during usage reporting.
    /// <see href="https://developers.deepgram.com/docs/tagging">
    /// Default is a null 
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("tag")]
    public List<string>? Tag { get; set; }

    /// <summary>
    /// Indicates how long Deepgram will wait to send a {"type": "UtteranceEnd"} message after a word has been transcribed
    /// <see href="https://developers.deepgram.com/docs/understanding-end-of-speech-detection-while-streaming"/>
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("utterance_end_ms")]
    public string? UtteranceEnd { get; set; }

    /// <summary>
    /// Enables voice activity detection (VAD) events
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("vad_events")]
    public bool? VadEvents { get; set; }

    /// <summary>
    /// Version of the model to use.
    /// <see href="https://developers.deepgram.com/docs/version">
    /// default value is "latest"
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("version")]
    public string? Version { get; set; }
}
