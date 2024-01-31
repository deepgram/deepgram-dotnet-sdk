namespace Deepgram.Models.Manage.v1;

public record Invite
{
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}
