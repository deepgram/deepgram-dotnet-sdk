namespace Deepgram.Interfaces;

public interface IInvitationClient : IBaseClient
{
    Task<InvitationList> ListInvitationsAsync(string projectId);
    Task<InvitationResponse> SendInvitationAsync(string projectId, InvitationOptions invitationOptions);
}
