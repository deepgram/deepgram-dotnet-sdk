namespace Deepgram.Interfaces;

public interface IInvitationClient
{
    Task<InvitationList> ListInvitationsAsync(string projectId);
    Task<InvitationResponse> SendInvitationAsync(string projectId, InvitationOptions invitationOptions);

    Task<MessageResponse> DeleteInvitationAsync(string projectId, string email);
}
