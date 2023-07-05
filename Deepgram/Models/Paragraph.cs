namespace Deepgram.Models;

public class Paragraph
{
    /// <summary>
    /// Array of Sentence objects.
    /// </summary>
    [JsonPropertyName("sentences")]
    public Sentence[] Sentences { get; set; }

    /// <summary>
    /// Offset in seconds from the start of the audio to where the paragraph starts.
    /// </summary>
    [JsonPropertyName("start")]
    public decimal Start { get; set; }

    /// <summary>
    /// Offset in seconds from the start of the audio to where the paragraph ends.
    /// </summary>
    [JsonPropertyName("end")]
    public decimal End { get; set; }

    /// <summary>
    /// Number of words in the paragraph
    /// </summary>
    [JsonPropertyName("num_words")]
    public int NumberOfWords { get; set; }
}
