namespace Deepgram.Models.Analyze.v1;

public record Summary
{
    /// <summary>
    /// Summary of a section of the transcript
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

