using System.Net.Http;
using System.Threading.Tasks;
using Deepgram.Common;
using Deepgram.Request;

namespace Deepgram.Invitation
{
    public sealed class InvitationClient : IInvitationClient
    {
        private ApiRequest _apiRequest;
        public InvitationClient(ApiRequest apiRequest)
        {
            _apiRequest = apiRequest;
        }


        public async Task<MessageResponse> DeleteInvitationAsync(string projectId, string email)
        {
            return await _apiRequest.DoRequestAsync<MessageResponse>(
                HttpMethod.Delete,
                $"projects/{projectId}/invites/{email}");
        }

        public async Task<InvitationList> ListInvitationsAsync(string projectId)
        {
            return await _apiRequest.DoRequestAsync<InvitationList>(
                  HttpMethod.Get,
                  $"projects/{projectId}/invites");
        }

        public async Task<InvitationResponse> SendInvitationAsync(string projectId, InvitationOptions invitationOptions)
        {
            return await _apiRequest.DoRequestAsync<InvitationResponse>(
          HttpMethod.Post,
          $"projects/{projectId}/invites",
          invitationOptions);

        }
    }
}
