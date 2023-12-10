namespace Deepgram.Models;

public class UrlSource
{
    /// <summary>
    /// Url of the file to transcribe
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// transcribable input in byte array
    /// </summary>
    public byte[]? Buffer { get; set; }

    /// <summary>
    /// Stream to transcribe
    /// </summary>
    public Stream? Stream { get; set; }
}

