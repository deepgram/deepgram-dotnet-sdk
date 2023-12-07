namespace Deepgram.Records;

public record GetProjectInvitesResponse
{
    [JsonPropertyName("invites")]
    public IReadOnlyList<Invite>? Invites { get; set; }
}
