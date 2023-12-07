using Deepgram.Records;
using Deepgram.Records.OnPrem;

namespace Deepgram;

/// <summary>
/// Constructor for the client that communicates with the on premise Deepgram Server
/// </summary>
/// <param name="httpClient"><see cref="HttpClient"/> for making Rest calls</param>
/// <param name="deepgramClientOptions"><see cref="DeepgramClientOptions"/> for HttpClient Configuration</param>
public class OnPremClient(DeepgramClientOptions deepgramClientOptions, HttpClient httpClient)
    : AbstractRestClient(deepgramClientOptions, httpClient)
{
    readonly string _urlPrefix = $"/{Constants.API_VERSION}/{Constants.PROJECTS}";
    /// <summary>
    /// get a list of credentials associated with project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns>ListOnPremCredentialsResponse</returns>
    public async Task<ListOnPremCredentialsResponse> ListCredentialsAsync(string projectId) =>
        await GetAsync<ListOnPremCredentialsResponse>($"{_urlPrefix}/{projectId}/{Constants.ONPREM}");

    /// <summary>
    /// Get credentials for the project that is associated with credential ID
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="credentialsId">Id of credentials</param>
    /// <returns>OnPremCredentialResponse</returns>
    public async Task<OnPremCredentialResponse> GetCredentialsAsync(string projectId, string credentialsId) =>
        await GetAsync<OnPremCredentialResponse>($"{_urlPrefix}/{projectId}/{Constants.ONPREM}/{credentialsId}");

    /// <summary>
    /// Remove credentials in the project associated with the credentials ID
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="credentialsId">Id of credentials</param>
    /// <returns>Message Response</returns>
    public async Task<MessageResponse> DeleteCredentialsAsync(string projectId, string credentialsId) =>
        await DeleteAsync<MessageResponse>($"{_urlPrefix}/{projectId}/{Constants.ONPREM}/{credentialsId}");

    /// <summary>
    /// Create credentials for the associated projects
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="createOnPremCredentialsSchema"><see cref="CreateOnPremCredentialsSchema"/> for credentials to be created</param>
    /// <returns>OnPremCredentialResponse</returns>
    public async Task<OnPremCredentialResponse> CreateCredentialsAsync(string projectId, CreateOnPremCredentialsSchema createOnPremCredentialsSchema) =>
        await PostAsync<OnPremCredentialResponse>(
            $"{_urlPrefix}/{projectId}/{Constants.ONPREM}",
            RequestContentUtil.CreatePayload(createOnPremCredentialsSchema));

}
