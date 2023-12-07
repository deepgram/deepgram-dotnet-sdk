namespace Deepgram.Models;

public class PrerecordedSchema : TranscriptionSchema
{
    /// <summary>
    /// Entity Detection identifies and extracts key entities from content in submitted audio
    /// <see href="https://developers.deepgram.com/docs/detect-entities">
    /// </summary>
    [JsonPropertyName("dectect_entites")]
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
    /// </summary>
    [JsonPropertyName("detect_topics")]
    public bool? DetectTopics { get; set; }


    [JsonPropertyName("alternatives")]
    public int? Alternatives { get; set; }

    /// <summary>
    /// Paragraphs splits audio into paragraphs to improve transcript readability.
    /// <see href="https://developers.deepgram.com/docs/paragraphs">
    /// </summary>
    [JsonPropertyName("paragraphs")]
    public bool? Paragraphs { get; set; }

    /// <summary>
    /// Summarizes content of submitted audio. 
    /// <see href="https://developers.deepgram.com/docs/summarization">
    /// </summary>
    [JsonPropertyName("summarize")]
    public object? Summarize { get; set; }

    /// <summary>
    /// Utterances segments speech into meaningful semantic units.
    /// <see href="https://developers.deepgram.com/docs/utterances">
    /// </summary>
    [JsonPropertyName("utterances")]
    public bool? Utterances { get; set; }

    /// <summary>
    /// Utterance Split detects pauses between words in submitted audio. 
    /// Used when the Utterances feature is enabled for pre-recorded audio.
    /// <see href="https://developers.deepgram.com/docs/utterance-split">
    /// </summary>
    [JsonPropertyName("utt_split")]
    public double? UttSplit { get; set; }
}
