﻿namespace Deepgram;

/// <summary>
/// Client for OnPrem user to manage credentials
/// </summary>
public class OnPremClient : AbstractRestClient
{
    /// <summary>
    /// Constructor that take a IHttpClientFactory
    /// </summary>
    /// <param name="apiKey">ApiKey used for Authentication Header and is required</param>
    /// <param name="httpClientFactory">IHttpClientFactory for creating instances of HttpClient for making Rest calls</param>
    internal OnPremClient(string? apiKey, IHttpClientFactory httpClientFactory)
        : base(apiKey, httpClientFactory, nameof(OnPremClient)) { }


    /// <summary>
    /// get a list of credentials associated with project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns>ListOnPremCredentialsResponse</returns>
    public async Task<ListOnPremCredentialsResponse> ListCredentialsAsync(string projectId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.ONPREM}";
        return await GetAsync<ListOnPremCredentialsResponse>(url);
    }

    /// <summary>
    /// Get credentials for the project that is associated with credential ID
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="credentialsId">Id of credentials</param>
    /// <returns>OnPremCredentialResponse</returns>
    public async Task<OnPremCredentialResponse> GetCredentialsAsync(string projectId, string credentialsId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.ONPREM}/{credentialsId}";
        return await GetAsync<OnPremCredentialResponse>(url);
    }

    /// <summary>
    /// Remove credentials in the project associated with the credentials ID
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="credentialsId">Id of credentials</param>
    /// <returns>Message Response</returns>
    public async Task<MessageResponse> DeleteCredentialsAsync(string projectId, string credentialsId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.ONPREM}/{credentialsId}";
        return await DeleteAsync<MessageResponse>(url);
    }

    /// <summary>
    /// Create credentials for the associated projects
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="createOnPremCredentialsSchema">options for credentials to be created</param>
    /// <returns>OnPremCredentialResponse</returns>
    public async Task<OnPremCredentialResponse> CreateCredentialsAsync(string projectId, CreateOnPremCredentialsSchema createOnPremCredentialsSchema)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.ONPREM}";
        var payload = CreatePayload(createOnPremCredentialsSchema);
        return await PostAsync<OnPremCredentialResponse>(url, payload);
    }

}