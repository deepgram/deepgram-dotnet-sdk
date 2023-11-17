namespace Deepgram.Models;

public class UrlSource
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
    public byte[]? Buffer { get; set; }

    public Stream? Stream { get; set; }
}

