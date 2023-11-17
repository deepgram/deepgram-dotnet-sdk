namespace Deepgram.Clients;


// working of node sdk - https://github.com/deepgram/deepgram-node-sdk/blob/lo/beta-test-improvements/src/packages/ManageClient.ts

/// <summary>
///  Client containing methods for interacting with API's to manage project(s)
/// </summary>
public class ManageClient : AbstractRestClient
{
    /// <summary>
    /// Constructor that take a IHttpClientFactory
    /// </summary>
    /// <param name="apiKey">ApiKey used for Authentication Header and is required</param>
    /// <param name="clientOptions">Optional HttpClient for configuring the HttpClient</param>
    /// <param name="loggerName">nameof the descendent class</param>
    /// <param name="httpClientFactory">IHttpClientFactory for creating instances of HttpClient for making Rest calls</param>
    internal ManageClient(string? apiKey, DeepgramClientOptions clientOptions, IHttpClientFactory httpClientFactory)
        : base(apiKey, clientOptions, nameof(ManageClient), httpClientFactory) { }

    #region Projects

    /// <summary>
    /// Gets projects associated to ApiKey 
    /// </summary>
    /// <returns>GetProjectsResponse</returns>
    public async Task<GetProjectsResponse> GetProjects()
    {
        string url = $"{Constants.PROJECTS}";
        return await GetAsync<GetProjectsResponse>(url);
    }

    /// <summary>
    /// Gets project associated with project Id
    /// </summary>
    /// <param name="projectId">Id of Project</param>
    /// <returns>GetProjectResponse</returns>
    public async Task<GetProjectResponse> GetProject(string projectId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}";
        return await GetAsync<GetProjectResponse>(url);
    }

    /// <summary>
    /// Update a project associated with the projectID
    /// </summary>
    /// <param name="projectId">ID of project</param>
    /// <param name="updateProjectSchema">Update options for project</param>
    /// <returns>Message Response</returns>
    public async Task<MessageResponse> UpdateProject(string projectId, UpdateProjectSchema updateProjectSchema)
    {
        string url = $"{Constants.PROJECTS}/{projectId}";
        // USES PATCH
        return await PatchAsync<MessageResponse, UpdateProjectSchema>(url, updateProjectSchema);
    }

    /// <summary>
    /// Deletes a project, no response will be returned
    /// </summary>
    /// <param name="projectId">Id of project</param>
    public void DeleteProject(string projectId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}";

        // No response expected
        _ = DeleteAsync(url);
    }

    /// <summary>
    /// leave project associated with the project Id
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns>MessageResponse</returns>
    public async Task<MessageResponse> LeaveProject(string projectId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/leave";
        return await DeleteAsync<MessageResponse>(url);
    }

    #endregion

    #region ProjectKeys

    /// <summary>
    /// Get the keys associated with the project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns>GetProjectKeysResponse</returns>
    public async Task<GetProjectKeysResponse> GetProjectKeys(string projectId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.KEYS}";
        return await GetAsync<GetProjectKeysResponse>(url);
    }

    /// <summary>
    /// Get details of key associated with the key ID
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="keyId">Id of key</param>
    /// <returns>GetProjectKeyResponse</returns>
    public async Task<GetProjectKeyResponse> GetProjectKey(string projectId, string keyId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.KEYS}/{keyId}";
        return await GetAsync<GetProjectKeyResponse>(url);
    }

    /// <summary>
    /// Create a key in the associated project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="createProjectKeySchema">options for the key to be created</param>
    /// <returns>CreateProjectKeyResponse</returns>
    public async Task<CreateProjectKeyResponse> CreateProjectKey(string projectId, CreateProjectKeySchema createProjectKeySchema)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/keys";
        return await PostAsync<CreateProjectKeyResponse, CreateProjectKeySchema>(url, createProjectKeySchema);
    }

    /// <summary>
    /// Remove key from project, No response returned
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="keyId">Id of key</param>
    public void DeleteProjectKey(string projectId, string keyId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/keys/{keyId}";
        _ = DeleteAsync(url);
    }
    #endregion

    #region ProjectInvites
    /// <summary>
    /// Get any invites that are associated with project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns>GetProjectInvitesResponse</returns>
    public async Task<GetProjectInvitesResponse> GetProjectInvites(string projectId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.INVITES}";
        return await GetAsync<GetProjectInvitesResponse>(url);
    }

    /// <summary>
    /// Delete a project invite that has been sent
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="email">email of the invite to be removed</param>
    public void DeleteProjectInvite(string projectId, string email)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/invites/{email}";
        _ = DeleteAsync(url);
    }

    /// <summary>
    /// Send a invite to the associated project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="sendProjectInviteSchema">Details for a invite to project</param>
    /// <returns>MessageResponse</returns>
    public async Task<MessageResponse> SendProjectInvite(string projectId, SendProjectInviteSchema sendProjectInviteSchema)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/invites";
        return await PostAsync<MessageResponse, SendProjectInviteSchema>(url, sendProjectInviteSchema);
    }


    #endregion

    #region Members

    /// <summary>
    /// Get the members associated with the project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns>GetProjectMembersResponse</returns>
    public async Task<GetProjectMembersResponse> GetProjectMembers(string projectId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.MEMBERS}";
        return await GetAsync<GetProjectMembersResponse>(url);
    }

    /// <summary>
    /// Get the scopes associated with member
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="memberId">Id of member</param>
    /// <returns>GetProjectMembersScopesResponse</returns>
    public async Task<GetProjectMemberScopesResponse> GetProjectMemberScopes(string projectId, string memberId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.MEMBERS}/{memberId}/{Constants.SCOPES}";
        return await GetAsync<GetProjectMemberScopesResponse>(url);
    }

    /// <summary>
    /// Update the scopes fot the member
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="memberId">Id of member</param>
    /// <param name="updateProjectMemberScopeSchema">scope update options</param>
    /// <returns>MessageResponse</returns>  
    public async Task<MessageResponse> UpdateProjectMemberScope(string projectId, string memberId, UpdateProjectMemberScopeSchema updateProjectMemberScopeSchema)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.MEMBERS}/{memberId}/{Constants.SCOPES}";
        return await PutAsync<MessageResponse, UpdateProjectMemberScopeSchema>(url, updateProjectMemberScopeSchema);
    }

    /// <summary>
    /// Remove member from project, there is no response
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="memberId">Id of member</param>   
    public void RemoveProjectMember(string projectId, string memberId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.MEMBERS}/{memberId}";
        _ = DeleteAsync<MessageResponse>(url);
    }
    #endregion

    #region Usage

    /// <summary>
    /// Get usage request  associated with the project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="getProjectUsageRequestsSchema">constraints to limit the usage requests needed </param>
    /// <returns></returns>
    public async Task<GetProjectUsageRequestsResponse> GetProjectUsageRequests(string projectId, GetProjectUsageRequestsSchema getProjectUsageRequestsSchema)
    {
        var stringedOptions = QueryParameterUtil.GetParameters(getProjectUsageRequestsSchema);
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.REQUESTS}?{stringedOptions}";
        return await GetAsync<GetProjectUsageRequestsResponse>(url);
    }

    /// <summary>
    /// Get the details associated with the requestID
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="requestId">Id of request</param>
    /// <returns>GetProjectUsageResponse</returns>
    public async Task<GetProjectUsageRequestResponse> GetProjectUsageRequest(string projectId, string requestId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.REQUESTS}/{requestId}";
        return await GetAsync<GetProjectUsageRequestResponse>(url);
    }

    /// <summary>
    /// Gets a summary of usage
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="getProjectUsageSummarySchema">constraints to limit the usage requests for summarizing</param>
    /// <returns></returns>
    public async Task<GetProjectUsageSummaryResponse> GetProjectUsageSummary(string projectId, GetProjectUsageSummarySchema getProjectUsageSummarySchema)
    {
        var stringedOptions = QueryParameterUtil.GetParameters(getProjectUsageSummarySchema);
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.USAGE}?{stringedOptions}";
        return await GetAsync<GetProjectUsageSummaryResponse>(url);
    }

    /// <summary>
    /// Get usage fields 
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="getProjectUsageFieldsSchema">constraints on Usage request range</param>
    /// <returns>GetProjectUsageFieldsResponse</returns>
    public async Task<GetProjectUsageFieldsResponse> GetProjectUsageFields(string projectId, GetProjectUsageFieldsSchema getProjectUsageFieldsSchema)
    {
        var stringedOptions = QueryParameterUtil.GetParameters(getProjectUsageFieldsSchema);
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.USAGE}/fields?{stringedOptions}";
        return await GetAsync<GetProjectUsageFieldsResponse>(url);
    }
    #endregion

    #region Balances

    /// <summary>
    /// Gets a list of balances
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns>GetProjectBalancesResponse</returns>
    public async Task<GetProjectBalancesResponse> GetProjectBalances(string projectId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.BALANCES}";
        return await GetAsync<GetProjectBalancesResponse>(url);
    }

    /// <summary>
    /// Get the balance details associated with the balance id
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="balanceId">Id of balance</param>
    /// <returns>GetProjectBalanceResponse</returns>
    public async Task<GetProjectBalanceResponse> GetProjectBalance(string projectId, string balanceId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.BALANCES}/{balanceId}";
        return await GetAsync<GetProjectBalanceResponse>(url);
    }
    #endregion

}
