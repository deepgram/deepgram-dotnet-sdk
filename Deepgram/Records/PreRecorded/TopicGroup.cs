namespace Deepgram.Records.PreRecorded;

public record TopicGroup
{
    [JsonPropertyName("topics")]
    public IReadOnlyList<Topic>? Topics { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("start_word")]
    public int StartWord { get; set; }

    [JsonPropertyName("end_word")]
    public int EndWord { get; set; }
}

