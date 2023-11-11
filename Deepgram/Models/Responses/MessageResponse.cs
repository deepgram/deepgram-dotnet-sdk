namespace Deepgram.Models.Responses;

public class MessageResponse
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
