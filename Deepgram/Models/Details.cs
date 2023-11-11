namespace Deepgram.Models;

public class Details
{
    [JsonPropertyName("usd")]
    public double? Usd { get; set; }

    [JsonPropertyName("duration")]
    public double? Duration { get; set; }

    [JsonPropertyName("total_audio")]
    public double? TotalAudio { get; set; }

    [JsonPropertyName("channels")]
    public int? Channels { get; set; }

    [JsonPropertyName("streams")]
    public int? Streams { get; set; }

    [JsonPropertyName("models")]
    public string[]? Models { get; set; }

    [JsonPropertyName("method")]
    public string? Method { get; set; }

    [JsonPropertyName("tags")]
    public string[]? Tags { get; set; }

    [JsonPropertyName("features")]
    public string[]? Features { get; set; }

    [JsonPropertyName("config")]
    public Config? Config { get; set; }
}