namespace Deepgram.Clients;

public class OnPremClient : AbstractRestClient
{
    /// <summary>
    /// Constructor that take a IHttpClientFactory
    /// </summary>
    /// <param name="apiKey">ApiKey used for Authentication Header and is required</param>
    /// <param name="clientOptions">Optional HttpClient for configuring the HttpClient</param>   
    /// <param name="httpClientFactory">IHttpClientFactory for creating instances of HttpClient for making Rest calls</param>
    internal OnPremClient(string? apiKey, DeepgramClientOptions clientOptions, IHttpClientFactory httpClientFactory)
        : base(apiKey, clientOptions, nameof(OnPremClient), httpClientFactory)
    {

    }

    public async Task<ListOnPremCredentialsResponse> ListCredentials(string projectId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.ONPREM}";
        throw new NotImplementedException();
    }
    public async Task<OnPremCredentialResponse> GetCredentials(string projectId, string credentialsId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.ONPREM}/{credentialsId}";
        throw new NotImplementedException();
    }
    public async Task<MessageResponse> DeleteCredentials(string projectId, string credentialsId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.ONPREM}/{credentialsId}";
        throw new NotImplementedException();
    }

    public async Task<OnPremCredentialResponse> CreateCredentials(string projectId, CreateOnPremCredentialsSchema createOnPremCredentialsSchema)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.ONPREM}";
        throw new NotImplementedException();
    }

}
