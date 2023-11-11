namespace Deepgram.Models;

public class Warning
{
    [JsonPropertyName("parameter")]
    public string? Parameter { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

