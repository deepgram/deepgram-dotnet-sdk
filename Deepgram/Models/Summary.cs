namespace Deepgram.Models;

public class Summary
{
    /// <summary>
    /// Summary of a section of the transcript
    /// </summary>
    [JsonPropertyName("summary")]
    public string TextSummary { get; set; }

    /// <summary>
    /// Word position in transcript where the summary begins
    /// </summary>
    [JsonPropertyName("start_word")]
    public int StartWord { get; set; }

    /// <summary>
    /// Word position in transcript where the summary ends
    /// </summary>
    [JsonPropertyName("end_word")]
    public int EndWord { get; set; }

    /// <summary>
    /// Array of Channel objects.
    /// </summary>
    [JsonPropertyName("short")]
    public string Short { get; set; }

}


