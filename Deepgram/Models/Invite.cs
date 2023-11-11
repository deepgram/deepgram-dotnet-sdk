namespace Deepgram.Models;

public class Invite
{
    [JsonPropertyName("email")]
    string? Email { get; set; }

    [JsonPropertyName("scope")]
    string? Scope { get; set; }
}