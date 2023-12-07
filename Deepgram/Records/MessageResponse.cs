namespace Deepgram.Records;

public record MessageResponse
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
