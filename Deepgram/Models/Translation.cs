namespace Deepgram.Models;

public class Translation
{
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("translation")]
    public string? Text { get; set; }
}

