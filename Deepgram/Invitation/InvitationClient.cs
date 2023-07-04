using System.Net.Http;
using System.Threading.Tasks;
using Deepgram.Common;
using Deepgram.Request;
using Deepgram.Utilities;

namespace Deepgram.Invitation
{
    internal class InvitationClient : IInvitationClient
    {

        private CleanCredentials _credentials;
        public ApiRequest _apiRequest;
        internal InvitationClient(CleanCredentials credentials)
        {
            _credentials = credentials;
            _apiRequest = new ApiRequest(HttpClientUtil.HttpClient);
        }

        public async Task<MessageResponse> DeleteInvitationAsync(string projectId, string email)
        {

            return await _apiRequest.DoRequestAsync<MessageResponse>(
                HttpMethod.Delete,
                $"projects/{projectId}/invites/{email}",
                _credentials);
        }

        public async Task<InvitationList> ListInvitationsAsync(string projectId)
        {
            return await _apiRequest.DoRequestAsync<InvitationList>(
                  HttpMethod.Get,
                  $"projects/{projectId}/invites",
                  _credentials
              );
        }

        public async Task<InvitationResponse> SendInvitationAsync(string projectId, InvitationOptions invitationOptions)
        {
            return await _apiRequest.DoRequestAsync<InvitationResponse>(
          HttpMethod.Post,
          $"projects/{projectId}/invites",
          _credentials,
          invitationOptions);

        }
    }
}
