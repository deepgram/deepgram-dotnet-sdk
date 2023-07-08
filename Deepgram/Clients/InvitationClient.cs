namespace Deepgram.Clients;

public class InvitationClient : IInvitationClient
{
    private ApiRequest _apiRequest;
    public InvitationClient(ApiRequest apiRequest)
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
