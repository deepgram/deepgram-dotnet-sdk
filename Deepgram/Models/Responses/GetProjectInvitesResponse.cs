namespace Deepgram.Models.Responses;

public class GetProjectInvitesResponse
{
    [JsonPropertyName("invites")]
    public Invite[]? Invites { get; set; }
}
