namespace Deepgram.Clients;

public class BillingClient : IBillingClient
{
    private ApiRequest _apiRequest;
    internal BillingClient(ApiRequest apiRequest)
    {
        _apiRequest = apiRequest;
    }

    /// <summary>
    /// Generates a list of outstanding balances for the specified project. To see balances, the authenticated account must be a project owner or administrator
    /// </summary>
    /// <param name="projectId">Unique identifier of the project for which you want to retrieve outstanding balances</param>
    /// <returns>List of Deepgram balances</returns>
    public async Task<BillingList> GetAllBalancesAsync(string projectId)
        => await _apiRequest.SendHttpRequestAsync<BillingList>(
                HttpMethod.Get,
                $"projects/{projectId}/balances");


    /// <summary>
    /// Retrieves details about the specified balance. To see balances, the authenticated account must be a project owner or administrator
    /// </summary>
    /// <param name="projectId">Unique identifier of the project for which you want to retrieve the specified balance</param>
    /// <param name="balanceId">Unique identifier of the balance that you want to retrieve</param>
    /// <returns>A Deepgram balance</returns>
    public async Task<Billing> GetBalanceAsync(string projectId, string balanceId) =>
        await _apiRequest.SendHttpRequestAsync<Billing>(
                HttpMethod.Get,
                $"projects/{projectId}/balances/{balanceId}");
}
