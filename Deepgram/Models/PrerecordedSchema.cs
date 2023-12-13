namespace Deepgram.Models;

public class PrerecordedSchema : TranscriptionSchema
{


    /// <summary>
    /// Entity Detection identifies and extracts key entities from content in submitted audio
    /// <see href="https://developers.deepgram.com/docs/detect-entities">
    /// </summary>
    [JsonPropertyName("detect_entites")]
    public bool? DetectEntities { get; set; }

    /// <summary>
    /// Language Detection identifies the dominant language spoken in submitted audio.
    /// <see href="https://developers.deepgram.com/docs/language-detection">
    /// </summary>
    [JsonPropertyName("detect_language")]
    public bool DetectLanguage { get; set; } = default;

    /// <summary>
    /// Topic Detection identifies and extracts key topics from content in submitted audio. 
    /// <see href="https://developers.deepgram.com/docs/topic-detection">
    /// Default is false
    /// </summary>
    [JsonPropertyName("detect_topics")]
    public bool DetectTopics { get; set; } = default;

    /// <summary>
    /// Spoken dictation commands will be converted to their corresponding punctuation marks. e.g., comma to ,
    /// <see href="https://developers.deepgram.com/reference/pre-recorded"/>
    /// Default is false
    /// </summary>
    [JsonPropertyName("dictation")]
    public bool Dictation { get; set; } = false;

    /// <summary>
    /// Spoken measurements will be converted to their corresponding abbreviations. e.g., milligram to mg
    /// Default is false
    /// </summary>
    [JsonPropertyName("measurements")]
    public bool Measurements { get; set; } = false;

    [JsonPropertyName("ner")]
    [Obsolete("Replaced with SmartFormat")]
    public bool? Ner { get; set; }

    /// <summary>
    /// Paragraphs splits audio into paragraphs to improve transcript readability.
    /// <see href="https://developers.deepgram.com/docs/paragraphs">
    /// </summary>
    [JsonPropertyName("paragraphs")]
    public bool Paragraphs { get; set; } = default;

    [JsonPropertyName("sentiment")]
    public bool Sentiment { get; set; }

    [JsonPropertyName("sentiment_threshold")]
    public double SentimentThreshold { get; set; }

    /// <summary>
    /// Summarizes content of submitted audio. 
    /// <see href="https://developers.deepgram.com/docs/summarization">
    /// Default is v2
    /// </summary>
    [JsonPropertyName("summarize")]
    public object Summarize { get; set; } = "v2";

    /// <summary>
    /// Utterances segments speech into meaningful semantic units.
    /// <see href="https://developers.deepgram.com/docs/utterances">
    /// default is false
    /// </summary>
    [JsonPropertyName("utterances")]
    public bool Utterances { get; set; } = default;

    /// <summary>
    /// Utterance Split detects pauses between words in submitted audio. 
    /// Used when the Utterances feature is enabled for pre-recorded audio.
    /// <see href="https://developers.deepgram.com/docs/utterance-split">
    /// Default is 0.8
    /// </summary>
    [JsonPropertyName("utt_split")]
    public double UttSplit { get; set; } = 0.8;

}
