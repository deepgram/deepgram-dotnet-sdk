namespace Deepgram.Models.Analyze.v1;

public class UrlSource(string url)
{
    /// <summary>
    /// Url of the file to transcribe
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; } = url;
}

