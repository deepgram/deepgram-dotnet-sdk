namespace Deepgram.Models;

public class TopicList
{
    /// <summary>
    /// Transcript covered by the topic.
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; set; }

    /// <summary>
    /// Word position in transcript where the topic begins
    /// </summary>
    [JsonPropertyName("start_word")]
    public int StartWord { get; set; }

    /// <summary>
    /// Word position in transcript where the topic ends
    /// </summary>
    [JsonPropertyName("end_word")]
    public int EndWord { get; set; }

    /// <summary>
    /// Array of Topics identified.
    /// </summary>
    [JsonPropertyName("topics")]
    public Topic[] Topics { get; set; }
}
