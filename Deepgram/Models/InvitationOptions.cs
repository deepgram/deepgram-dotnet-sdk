namespace Deepgram.Models;

public class InvitationOptions
{
    [JsonProperty("email")]
    public string Email { get; set; }
    [JsonProperty("scope")]
    public string Scope { get; set; }
}
