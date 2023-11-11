namespace Deepgram.Models.Schemas;

public class PrerecordedSchema : TranscriptionSchema
{
    [JsonPropertyName("dectect_entites")]
    public bool? DetectEntities { get; set; }

    [JsonPropertyName("detect_language")]
    public bool? DetectLanguage { get; set; }

    [JsonPropertyName("detect_topics")]
    public bool? DetectTopics { get; set; }

    [JsonPropertyName("alternatives")]
    public int? Alternatives { get; set; }

    [JsonPropertyName("paragraphs")]
    public bool? Paragraphs { get; set; }

    [JsonPropertyName("summarize")]
    public object? Summarize { get; set; }

    [JsonPropertyName("utterances")]
    public bool? Utterances { get; set; }

    [JsonPropertyName("utt_split")]
    public double? UtteranceSplit { get; set; }
}
