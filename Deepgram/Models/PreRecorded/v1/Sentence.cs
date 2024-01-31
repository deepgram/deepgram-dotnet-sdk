namespace Deepgram.Models.PreRecorded.v1;

public record Sentence
{
    /// <summary>
    /// Text transcript of the sentence.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Offset in seconds from the start of the audio to where the sentence starts.
    /// </summary>
    [JsonPropertyName("start")]
    public decimal? Start { get; set; }

    /// <summary>
    /// Offset in seconds from the start of the audio to where the sentence ends.
    /// </summary>
    [JsonPropertyName("end")]
    public decimal? End { get; set; }

    [JsonPropertyName("sentiments")]
    public List<SentimentGroup> Sentiments;
}

