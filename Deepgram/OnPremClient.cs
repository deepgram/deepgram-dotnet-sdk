namespace Deepgram;

/// <summary>
/// Constructor that take a IHttpClientFactory
/// </summary>
/// <param name="apiKey">ApiKey used for Authentication Header and is required</param> 
/// <param name="httpClientFactory"><see cref="IHttpClientFactory"/> for creating instances of HttpClient for making Rest calls</param>
/// <param name="deepgramClientOptions"><see cref="DeepgramClientOptions"/> for HttpClient Configuration</param>
public class OnPremClient(string? apiKey, IHttpClientFactory httpClientFactory, DeepgramClientOptions? deepgramClientOptions = null)
    : AbstractRestClient(apiKey, httpClientFactory, deepgramClientOptions)
{

    /// <summary>
    /// get a list of credentials associated with project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns>ListOnPremCredentialsResponse</returns>
    public async Task<ListOnPremCredentialsResponse> ListCredentialsAsync(string projectId) =>
        await GetAsync<ListOnPremCredentialsResponse>($"{Constants.PROJECTS}/{projectId}/{Constants.ONPREM}");

    /// <summary>
    /// Get credentials for the project that is associated with credential ID
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="credentialsId">Id of credentials</param>
    /// <returns>OnPremCredentialResponse</returns>
    public async Task<OnPremCredentialResponse> GetCredentialsAsync(string projectId, string credentialsId) =>
        await GetAsync<OnPremCredentialResponse>($"{Constants.PROJECTS}/{projectId}/{Constants.ONPREM}/{credentialsId}");

    /// <summary>
    /// Remove credentials in the project associated with the credentials ID
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="credentialsId">Id of credentials</param>
    /// <returns>Message Response</returns>
    public async Task<MessageResponse> DeleteCredentialsAsync(string projectId, string credentialsId) =>
        await DeleteAsync<MessageResponse>($"{Constants.PROJECTS}/{projectId}/{Constants.ONPREM}/{credentialsId}");

    /// <summary>
    /// Create credentials for the associated projects
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="createOnPremCredentialsSchema"><see cref="CreateOnPremCredentialsSchema"/> for credentials to be created</param>
    /// <returns>OnPremCredentialResponse</returns>
    public async Task<OnPremCredentialResponse> CreateCredentialsAsync(string projectId, CreateOnPremCredentialsSchema createOnPremCredentialsSchema) =>
        await PostAsync<OnPremCredentialResponse>(
            $"{Constants.PROJECTS}/{projectId}/{Constants.ONPREM}",
            RequestContentUtil.CreatePayload(createOnPremCredentialsSchema));

}
