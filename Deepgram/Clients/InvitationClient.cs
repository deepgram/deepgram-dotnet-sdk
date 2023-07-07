namespace Deepgram.Clients;

public class InvitationClient : IInvitationClient
{
    internal IApiRequest _apiRequest;
    public InvitationClient(IApiRequest apiRequest)
    {
        _apiRequest = apiRequest;
    }

    public async Task<MessageResponse> DeleteInvitationAsync(string projectId, string email) =>
        await _apiRequest.SendHttpRequestAsync<MessageResponse>(
                HttpMethod.Delete,
                $"projects/{projectId}/invites/{email}");

    public async Task<InvitationList> ListInvitationsAsync(string projectId) =>
        await _apiRequest.SendHttpRequestAsync<InvitationList>(
                HttpMethod.Get,
                $"projects/{projectId}/invites");

    public async Task<InvitationResponse> SendInvitationAsync(string projectId, InvitationOptions invitationOptions)
        => await _apiRequest.SendHttpRequestAsync<InvitationResponse>(
            HttpMethod.Post,
            $"projects/{projectId}/invites",
            invitationOptions);
}
