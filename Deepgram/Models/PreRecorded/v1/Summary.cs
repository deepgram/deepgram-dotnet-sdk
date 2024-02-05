namespace Deepgram.Models.PreRecorded.v1;

public record Summary // aka SummaryV2
{
    /// <summary>
    /// Summary of a section of the transcript
    /// </summary>
    [JsonPropertyName("short")]
    public string? Short { get; set; }

    /// <summary>
    /// Word position in transcript where the summary begins
    /// </summary>
    [JsonPropertyName("result")]
    public string? Result { get; set; }
}

