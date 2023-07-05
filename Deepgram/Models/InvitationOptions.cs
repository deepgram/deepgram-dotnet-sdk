namespace Deepgram.Models;

public class InvitationOptions
{
    [JsonPropertyName("email")]
    public string Email { get; set; }
    [JsonPropertyName("scope")]
    public string Scope { get; set; }
}
