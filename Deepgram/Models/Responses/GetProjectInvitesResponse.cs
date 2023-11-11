namespace Deepgram.Models.Responses;

public class GetProjectInvitesResponse
{
    [JsonPropertyName("invites")]
    Invite[]? Invites { get; set; }
}