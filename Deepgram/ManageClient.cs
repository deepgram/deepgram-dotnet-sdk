namespace Deepgram;

/// <summary>
///  Client containing methods for interacting with API's to manage project(s)
/// </summary>
/// <param name="apiKey">ApiKey used for Authentication Header and is required</param>
/// <param name="httpClientFactory"><see cref="IHttpClientFactory"/> for creating instances of HttpClient for making Rest calls</param>
/// <param name="deepgramClientOptions"><see cref="DeepgramClientOptions"/> for HttpClient Configuration</param>
public class ManageClient(string? apiKey, IHttpClientFactory httpClientFactory, DeepgramClientOptions? deepgramClientOptions = null)
    : AbstractRestClient(apiKey, httpClientFactory, deepgramClientOptions)
{

    #region Projects
    /// <summary>
    /// Gets projects associated to ApiKey 
    /// </summary>
    /// <returns><see cref="GetProjectsResponse"/></returns>
    public async Task<GetProjectsResponse> GetProjectsAsync() =>
        await GetAsync<GetProjectsResponse>($"/{Constants.API_VERSION}/{Constants.PROJECTS}");

    /// <summary>
    /// Gets project associated with project Id
    /// </summary>
    /// <param name="projectId">Id of Project</param>
    /// <returns><see cref="GetProjectResponse"/></returns>
    public async Task<GetProjectResponse> GetProjectAsync(string projectId) =>
        await GetAsync<GetProjectResponse>($"{Constants.PROJECTS}/{projectId}");

    /// <summary>
    /// Update a project associated with the projectID
    /// </summary>
    /// <param name="projectId">ID of project</param>
    /// <param name="updateProjectSchema"><see cref="UpdateProjectSchema"/> for project</param>
    /// <returns><see cref="MessageResponse"/></returns>
    // USES PATCH
    public async Task<MessageResponse> UpdateProjectAsync(string projectId, UpdateProjectSchema updateProjectSchema) =>
        await PatchAsync<MessageResponse>(
            $"{Constants.PROJECTS}/{projectId}",
            CreatePayload(updateProjectSchema));

    /// <summary>
    /// Deletes a project, no response will be returned
    /// </summary>
    /// <param name="projectId">Id of project</param>
    // No response expected
    public void DeleteProject(string projectId) =>
        _ = Delete($"{Constants.PROJECTS}/{projectId}");

    /// <summary>
    /// leave project associated with the project Id
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="MessageResponse"/></returns>
    public async Task<MessageResponse> LeaveProjectAsync(string projectId) =>
        await DeleteAsync<MessageResponse>($"{Constants.PROJECTS}/{projectId}/leave");

    #endregion

    #region ProjectKeys

    /// <summary>
    /// Get the keys associated with the project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="GetProjectKeysResponse"/></returns>
    public async Task<GetProjectKeysResponse> GetProjectKeysAsync(string projectId) =>
        await GetAsync<GetProjectKeysResponse>($"{Constants.PROJECTS}/{projectId}/{Constants.KEYS}");

    /// <summary>
    /// Get details of key associated with the key ID
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="keyId">Id of key</param>
    /// <returns><see cref="GetProjectKeyResponse"/></returns>
    public async Task<GetProjectKeyResponse> GetProjectKeyAsync(string projectId, string keyId) =>
        await GetAsync<GetProjectKeyResponse>($"{Constants.PROJECTS}/{projectId}/{Constants.KEYS}/{keyId}");

    /// <summary>
    /// Create a key in the associated project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="createProjectKeySchema"><see cref="CreateProjectKeySchema"/> for the key to be created</param>
    /// <returns><see cref="CreateProjectKeyResponse"/></returns>
    public async Task<CreateProjectKeyResponse> CreateProjectKeyAsync(string projectId, CreateProjectKeySchema createProjectKeySchema) =>
         await PostAsync<CreateProjectKeyResponse>(
             $"{Constants.PROJECTS}/{projectId}/keys",
             CreatePayload(createProjectKeySchema));


    /// <summary>
    /// Remove key from project, No response returned
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="keyId">Id of key</param>
    public void DeleteProjectKey(string projectId, string keyId) =>
        _ = Delete($"{Constants.PROJECTS}/{projectId}/keys/{keyId}");

    #endregion

    #region ProjectInvites
    /// <summary>
    /// Get any invites that are associated with project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="GetProjectInvitesResponse"/></returns>
    public async Task<GetProjectInvitesResponse> GetProjectInvitesAsync(string projectId) =>
        await GetAsync<GetProjectInvitesResponse>($"{Constants.PROJECTS}/{projectId}/{Constants.INVITES}");

    /// <summary>
    /// Delete a project invite that has been sent
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="email">email of the invite to be removed</param>
    public void DeleteProjectInvite(string projectId, string email) =>
        _ = Delete($"{Constants.PROJECTS}/{projectId}/{Constants.INVITES}/{email}");

    /// <summary>
    /// Send a invite to the associated project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="sendProjectInviteSchema"><see cref="SendProjectInviteSchema"/> for a invite to project</param>
    /// <returns><see cref="MessageResponse"/></returns>
    public async Task<MessageResponse> SendProjectInviteAsync(string projectId, SendProjectInviteSchema sendProjectInviteSchema) =>
        await PostAsync<MessageResponse>(
            $"{Constants.PROJECTS}/{projectId}/{Constants.INVITES}",
            CreatePayload(sendProjectInviteSchema));
    #endregion

    #region Members
    /// <summary>
    /// Get the members associated with the project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="GetProjectMembersResponse"/></returns>
    public async Task<GetProjectMembersResponse> GetProjectMembersAsync(string projectId) =>
        await GetAsync<GetProjectMembersResponse>($"{Constants.PROJECTS}/{projectId}/{Constants.MEMBERS}");

    /// <summary>
    /// Get the scopes associated with member
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="memberId">Id of member</param>
    /// <returns><see cref="GetProjectMemberScopesResponse"/></returns>
    public async Task<GetProjectMemberScopesResponse> GetProjectMemberScopesAsync(string projectId, string memberId) =>
        await GetAsync<GetProjectMemberScopesResponse>($"{Constants.PROJECTS}/{projectId}/{Constants.MEMBERS}/{memberId}/{Constants.SCOPES}");

    /// <summary>
    /// Update the scopes fot the member
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="memberId">Id of member</param>
    /// <param name="updateProjectMemberScopeSchema"><see cref="UpdateProjectMemberScopeSchema"/> for member</param>
    /// <returns><see cref="MessageResponse"/></returns>  
    public async Task<MessageResponse> UpdateProjectMemberScopeAsync(string projectId, string memberId, UpdateProjectMemberScopeSchema updateProjectMemberScopeSchema) =>
        await PutAsync<MessageResponse>(
            $"{Constants.PROJECTS}/{projectId}/{Constants.MEMBERS}/{memberId}/{Constants.SCOPES}",
            CreatePayload(updateProjectMemberScopeSchema));

    /// <summary>
    /// Remove member from project, there is no response
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="memberId">Id of member</param>   
    public void RemoveProjectMember(string projectId, string memberId) =>
        _ = Delete($"{Constants.PROJECTS}/{projectId}/{Constants.MEMBERS}/{memberId}");
    #endregion

    #region Usage

    /// <summary>
    /// Get usage request  associated with the project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="getProjectUsageRequestsSchema"><see cref="GetProjectUsageRequestsSchema"/>  </param>
    /// <returns><see cref="GetProjectUsageRequestsResponse"/></returns>
    public async Task<GetProjectUsageRequestsResponse> GetProjectsUsageRequestsAsync(string projectId, GetProjectUsageRequestsSchema getProjectUsageRequestsSchema)
    {
        var stringedOptions = QueryParameterUtil.GetParameters(getProjectUsageRequestsSchema);
        return await GetAsync<GetProjectUsageRequestsResponse>($"{Constants.PROJECTS}/{projectId}/{Constants.REQUESTS}?{stringedOptions}");
    }

    /// <summary>
    /// Get the details associated with the requestID
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="requestId">Id of request</param>
    /// <returns><see cref="GetProjectUsageRequestResponse"/></returns>
    public async Task<GetProjectUsageRequestResponse> GetProjectUsageRequestAsync(string projectId, string requestId) =>
        await GetAsync<GetProjectUsageRequestResponse>($"{Constants.PROJECTS}/{projectId}/{Constants.REQUESTS}/{requestId}");

    /// <summary>
    /// Gets a summary of usage
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="getProjectUsageSummarySchema"><see cref="GetProjectsUsageSummarySchema"/> constraints to limit the usage requests for summarizing</param>
    /// <returns><see cref="GetProjectUsageSummaryResponse"/></returns>
    public async Task<GetProjectUsageSummaryResponse> GetProjectUsageSummaryAsync(string projectId, GetProjectsUsageSummarySchema getProjectUsageSummarySchema)
    {
        var stringedOptions = QueryParameterUtil.GetParameters(getProjectUsageSummarySchema);
        return await GetAsync<GetProjectUsageSummaryResponse>(
            $"{Constants.PROJECTS}/{projectId}/{Constants.USAGE}?{stringedOptions}");
    }

    /// <summary>
    /// Get usage fields 
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="getProjectUsageFieldsSchema"><see cref="GetProjectUsageFieldsSchema"/> constraints on Usage request range</param>
    /// <returns><see cref="GetProjectUsageFieldsResponse"/></returns>
    public async Task<GetProjectUsageFieldsResponse> GetProjectUsageFieldsAsync(string projectId, GetProjectUsageFieldsSchema getProjectUsageFieldsSchema)
    {
        var stringedOptions = QueryParameterUtil.GetParameters(getProjectUsageFieldsSchema);
        return await GetAsync<GetProjectUsageFieldsResponse>(
            $"{Constants.PROJECTS}/{projectId}/{Constants.USAGE}/fields?{stringedOptions}");
    }
    #endregion

    #region Balances

    /// <summary>
    /// Gets a list of balances
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="GetProjectBalancesResponse"/></returns>
    public async Task<GetProjectBalancesResponse> GetProjectBalancesAsync(string projectId) =>
        await GetAsync<GetProjectBalancesResponse>($"{Constants.PROJECTS}/{projectId}/{Constants.BALANCES}");

    /// <summary>
    /// Get the balance details associated with the balance id
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="balanceId">Id of balance</param>
    /// <returns><see cref="GetProjectBalanceResponse"/></returns>
    public async Task<GetProjectBalanceResponse> GetProjectBalanceAsync(string projectId, string balanceId) =>
        await GetAsync<GetProjectBalanceResponse>($"{Constants.PROJECTS}/{projectId}/{Constants.BALANCES}/{balanceId}");
    #endregion
}
