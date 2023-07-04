using System.Net.Http;
using System.Threading.Tasks;
using Deepgram.Request;
using Deepgram.Utilities;

namespace Deepgram.Billing
{
    internal class BillingClient : IBillingClient
    {
        private CleanCredentials _credentials;
        private ApiRequest _apiRequest;

        public BillingClient(CleanCredentials credentials)
        {
            _credentials = credentials;
            _apiRequest = new ApiRequest(HttpClientUtil.HttpClient);
        }

        /// <summary>
        /// Generates a list of outstanding balances for the specified project. To see balances, the authenticated account must be a project owner or administrator
        /// </summary>
        /// <param name="projectId">Unique identifier of the project for which you want to retrieve outstanding balances</param>
        /// <returns>List of Deepgram balances</returns>
        public async Task<BillingList> GetAllBalancesAsync(string projectId)
        {
            return await _apiRequest.DoRequestAsync<BillingList>(
                   HttpMethod.Get,
                   $"projects/{projectId}/balances",
                   _credentials
               );
        }

        /// <summary>
        /// Retrieves details about the specified balance. To see balances, the authenticated account must be a project owner or administrator
        /// </summary>
        /// <param name="projectId">Unique identifier of the project for which you want to retrieve the specified balance</param>
        /// <param name="balanceId">Unique identifier of the balance that you want to retrieve</param>
        /// <returns>A Deepgram balance</returns>
        public async Task<Billing> GetBalanceAsync(string projectId, string balanceId)
        {
            return await _apiRequest.DoRequestAsync<Billing>(
                  HttpMethod.Get,
                  $"projects/{projectId}/balances/{balanceId}",
                  _credentials
              );
        }
    }
}
