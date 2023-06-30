using System.Net.Http;
using System.Threading.Tasks;
using Deepgram.Interfaces;
using Deepgram.Models;

namespace Deepgram.Clients
{
    public class BillingClient : BaseClient, IBillingClient
    {
        public BillingClient(Credentials credentials) : base(credentials) { }

        /// <summary>
        /// Generates a list of outstanding balances for the specified project. To see balances, the authenticated account must be a project owner or administrator
        /// </summary>
        /// <param name="projectId">Unique identifier of the project for which you want to retrieve outstanding balances</param>
        /// <returns>List of Deepgram balances</returns>
        public async Task<BillingList> GetAllBalancesAsync(string projectId)
        {
            var req = RequestMessageBuilder.CreateHttpRequestMessage(
                HttpMethod.Get,
                $"projects/{projectId}/balances",
                Credentials);

            return await ApiRequest.SendHttpRequestAsync<BillingList>(req);
        }

        /// <summary>
        /// Retrieves details about the specified balance. To see balances, the authenticated account must be a project owner or administrator
        /// </summary>
        /// <param name="projectId">Unique identifier of the project for which you want to retrieve the specified balance</param>
        /// <param name="balanceId">Unique identifier of the balance that you want to retrieve</param>
        /// <returns>A Deepgram balance</returns>
        public async Task<Billing> GetBalanceAsync(string projectId, string balanceId)
        {
            var req = RequestMessageBuilder.CreateHttpRequestMessage(
               HttpMethod.Get,
               $"projects/{projectId}/balances/{balanceId}",
               Credentials);

            return await ApiRequest.SendHttpRequestAsync<Billing>(req);
        }
    }
}
