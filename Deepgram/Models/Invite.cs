namespace Deepgram.Models;

public class Invite
{
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}
