using System.Net.Http;
using System.Threading.Tasks;
using Deepgram.Common;
using Deepgram.Request;

namespace Deepgram.Invitation
{
    public sealed class InvitationClient : BaseClient, IInvitationClient
    {
        public InvitationClient(CleanCredentials credentials) : base(credentials) { }


        public async Task<MessageResponse> DeleteInvitationAsync(string projectId, string email)
        {

            return await ApiRequest.DoRequestAsync<MessageResponse>(
                HttpMethod.Delete,
                $"projects/{projectId}/invites/{email}",
                Credentials);
        }

        public async Task<InvitationList> ListInvitationsAsync(string projectId)
        {
            return await ApiRequest.DoRequestAsync<InvitationList>(
                  HttpMethod.Get,
                  $"projects/{projectId}/invites",
                  Credentials
              );
        }

        public async Task<InvitationResponse> SendInvitationAsync(string projectId, InvitationOptions invitationOptions)
        {
            return await ApiRequest.DoRequestAsync<InvitationResponse>(
          HttpMethod.Post,
          $"projects/{projectId}/invites",
          Credentials,
          invitationOptions);

        }
    }
}
