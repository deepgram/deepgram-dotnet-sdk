namespace Deepgram.Clients;

public class InvitationClient : BaseClient, IInvitationClient
{
    public InvitationClient(CleanCredentials credentials) : base(credentials)
    {
    }

    public async Task<InvitationList> ListInvitationsAsync(string projectId)
    {
        var req = RequestMessageBuilder.CreateHttpRequestMessage(
      HttpMethod.Get,
      $"projects/{projectId}/invites",
      Credentials);

        return await ApiRequest.SendHttpRequestAsync<InvitationList>(req);

    }

    public async Task<InvitationResponse> SendInvitationAsync(string projectId, InvitationOptions invitationOptions)
    {
        var req = RequestMessageBuilder.CreateHttpRequestMessage(
      HttpMethod.Post,
      $"projects/{projectId}/invites",
      Credentials,
      invitationOptions);

        return await ApiRequest.SendHttpRequestAsync<InvitationResponse>(req);

    }
}
