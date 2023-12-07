namespace Deepgram.Records;

public record Invite
{
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}
