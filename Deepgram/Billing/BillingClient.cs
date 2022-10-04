using System.Net.Http;
using System.Threading.Tasks;
using Deepgram.Request;

namespace Deepgram.Billing
{
    internal class BillingClient: IBillingClient
    {
        private CleanCredentials _credentials;

        public BillingClient(CleanCredentials credentials)
        {
            _credentials = credentials;
        }

        /// <summary>
        /// Generates a list of outstanding balances for the specified project. To see balances, the authenticated account must be a project owner or administrator
        /// </summary>
        /// <param name="projectId">Unique identifier of the project for which you want to retrieve outstanding balances</param>
        /// <returns>List of Deepgram balances</returns>
        public async Task<Billing[]> GetAllBalancesAsync(string projectId)
        {
            return await ApiRequest.DoRequestAsync<Billing[]>(
                   HttpMethod.Get,
                   $"/v1/projects/{projectId}/balances",
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
            return await ApiRequest.DoRequestAsync<Billing>(
                  HttpMethod.Get,
                  $"/v1/projects/{projectId}/balances/{balanceId}",
                  _credentials
              );
        }
    }
}
