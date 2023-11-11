namespace Deepgram.Models;

public class Channel
{
    [JsonPropertyName("search")]
    public Search[]? Search { get; set; }
    [JsonPropertyName("alternatives")]
    public Alternative[]? Alternatives { get; set; }

    [JsonPropertyName("detected_language")]
    public string? DetectedLanguage { get; set; }
}
