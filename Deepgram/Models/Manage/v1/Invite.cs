namespace Deepgram.Models.Manage.v1;

public record Invite
{
    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}
