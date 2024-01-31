namespace Deepgram.Models.PreRecorded.v1;

public record Summary
{
    /// <summary>
    /// Summary of a section of the transcript
    /// </summary>
    [JsonPropertyName("summary")]
    public string? Text { get; set; }
    /// <summary>
    /// Word position in transcript where the summary begins
    /// </summary>
    [JsonPropertyName("start_word")]
    public int? StartWord { get; set; }
    /// <summary>
    /// Word position in transcript where the summary ends
    /// </summary>
    [JsonPropertyName("end_word")]
    public int? EndWord { get; set; }
}

