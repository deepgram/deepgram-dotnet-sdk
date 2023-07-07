namespace Deepgram.Clients;

public class InvitationClient : BaseClient, IInvitationClient
{
    public InvitationClient(CleanCredentials credentials) : base(credentials) { }

    public async Task<MessageResponse> DeleteInvitationAsync(string projectId, string email) =>
        await ApiRequest.SendHttpRequestAsync<MessageResponse>(
            RequestMessageBuilder.CreateHttpRequestMessage(
                HttpMethod.Delete,
                $"projects/{projectId}/invites/{email}",
                Credentials));

    public async Task<InvitationList> ListInvitationsAsync(string projectId) =>
        await ApiRequest.SendHttpRequestAsync<InvitationList>(
            RequestMessageBuilder.CreateHttpRequestMessage(
                HttpMethod.Get,
                $"projects/{projectId}/invites",
                Credentials));

    public async Task<InvitationResponse> SendInvitationAsync(string projectId, InvitationOptions invitationOptions)
        => await ApiRequest.SendHttpRequestAsync<InvitationResponse>
        (RequestMessageBuilder.CreateHttpRequestMessage(
            HttpMethod.Post,
            $"projects/{projectId}/invites",
            Credentials,
            invitationOptions));
}
