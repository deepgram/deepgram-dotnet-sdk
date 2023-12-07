using Deepgram.Records;

namespace Deepgram;

/// <summary>
///  Client containing methods for interacting with API's to manage project(s)
/// </summary>
/// <param name="httpClient"><see cref="HttpClient"/> for making Rest calls</param>
/// <param name="deepgramClientOptions"><see cref="DeepgramClientOptions"/> for HttpClient Configuration</param>
public class ManageClient(DeepgramClientOptions deepgramClientOptions, HttpClient httpClient)
    : AbstractRestClient(deepgramClientOptions, httpClient)
{
    readonly string _urlPrefix = $"/{Constants.API_VERSION}/{Constants.PROJECTS}";

    #region Projects
    /// <summary>
    /// Gets projects associated to ApiKey 
    /// </summary>
    /// <returns><see cref="GetProjectsResponse"/></returns>
    public async Task<GetProjectsResponse> GetProjectsAsync() =>
        await GetAsync<GetProjectsResponse>(_urlPrefix);

    /// <summary>
    /// Gets project associated with project Id
    /// </summary>
    /// <param name="projectId">Id of Project</param>
    /// <returns><see cref="GetProjectResponse"/></returns>
    public async Task<GetProjectResponse> GetProjectAsync(string projectId) =>
        await GetAsync<GetProjectResponse>($"{_urlPrefix}/{projectId}");

    /// <summary>
    /// Update a project associated with the projectID
    /// </summary>
    /// <param name="projectId">ID of project</param>
    /// <param name="updateProjectSchema"><see cref="UpdateProjectSchema"/> for project</param>
    /// <returns><see cref="MessageResponse"/></returns>
    // USES PATCH
    public async Task<MessageResponse> UpdateProjectAsync(string projectId, UpdateProjectSchema updateProjectSchema) =>
        await PatchAsync<MessageResponse>(
            $"{_urlPrefix}/{projectId}",
            RequestContentUtil.CreatePayload(updateProjectSchema));

    /// <summary>
    /// Deletes a project, no response will be returned
    /// </summary>
    /// <param name="projectId">Id of project</param>
    // No response expected
    public void DeleteProject(string projectId) =>
        _ = Delete($"{_urlPrefix}/{projectId}");

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
        await GetAsync<GetProjectKeysResponse>($"{_urlPrefix}/{projectId}/{Constants.KEYS}");

    /// <summary>
    /// Get details of key associated with the key ID
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="keyId">Id of key</param>
    /// <returns><see cref="GetProjectKeyResponse"/></returns>
    public async Task<GetProjectKeyResponse> GetProjectKeyAsync(string projectId, string keyId) =>
        await GetAsync<GetProjectKeyResponse>($"{_urlPrefix}/{projectId}/{Constants.KEYS}/{keyId}");

    /// <summary>
    /// Create a key in the associated project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="createProjectKeySchema"><see cref="CreateProjectKeySchema"/> for the key to be created</param>
    /// <returns><see cref="CreateProjectKeyResponse"/></returns>
    public async Task<CreateProjectKeyResponse> CreateProjectKeyAsync(string projectId, CreateProjectKeySchema createProjectKeySchema)
    {
        if (createProjectKeySchema.ExpirationDate is not null && createProjectKeySchema.TimeToLiveInSeconds is not null)
        {
            Log.CreateProjectKeyError(_logger, createProjectKeySchema);
            throw new ArgumentException("Both ExpirationDate and TimeToLiveInSeconds is set. set either one but not both");
        }

        return await PostAsync<CreateProjectKeyResponse>(
                     $"{_urlPrefix}/{projectId}/keys",
                     RequestContentUtil.CreatePayload(createProjectKeySchema));
    }


    /// <summary>
    /// Remove key from project, No response returned
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="keyId">Id of key</param>
    public void DeleteProjectKey(string projectId, string keyId) =>
        _ = Delete($"{_urlPrefix}/{projectId}/keys/{keyId}");

    #endregion

    #region ProjectInvites
    /// <summary>
    /// Get any invites that are associated with project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="GetProjectInvitesResponse"/></returns>
    public async Task<GetProjectInvitesResponse> GetProjectInvitesAsync(string projectId) =>
        await GetAsync<GetProjectInvitesResponse>($"{_urlPrefix}/{projectId}/{Constants.INVITES}");

    /// <summary>
    /// Delete a project invite that has been sent
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="email">email of the invite to be removed</param>
    public void DeleteProjectInvite(string projectId, string email) =>
        _ = Delete($"{_urlPrefix}/{projectId}/{Constants.INVITES}/{email}");

    /// <summary>
    /// Send a invite to the associated project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="sendProjectInviteSchema"><see cref="SendProjectInviteSchema"/> for a invite to project</param>
    /// <returns><see cref="MessageResponse"/></returns>
    public async Task<MessageResponse> SendProjectInviteAsync(string projectId, SendProjectInviteSchema sendProjectInviteSchema) =>
        await PostAsync<MessageResponse>(
            $"{_urlPrefix}/{projectId}/{Constants.INVITES}",
            RequestContentUtil.CreatePayload(sendProjectInviteSchema));
    #endregion

    #region Members
    /// <summary>
    /// Get the members associated with the project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="GetProjectMembersResponse"/></returns>
    public async Task<GetProjectMembersResponse> GetProjectMembersAsync(string projectId) =>
        await GetAsync<GetProjectMembersResponse>($"{_urlPrefix}/{projectId}/{Constants.MEMBERS}");

    /// <summary>
    /// Get the scopes associated with member
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="memberId">Id of member</param>
    /// <returns><see cref="GetProjectMemberScopesResponse"/></returns>
    public async Task<GetProjectMemberScopesResponse> GetProjectMemberScopesAsync(string projectId, string memberId) =>
        await GetAsync<GetProjectMemberScopesResponse>($"{_urlPrefix}/{projectId}/{Constants.MEMBERS}/{memberId}/{Constants.SCOPES}");

    /// <summary>
    /// Update the scopes fot the member
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="memberId">Id of member</param>
    /// <param name="updateProjectMemberScopeSchema">updates scope options for member<see cref="UpdateProjectMemberScopeSchema"/></param>
    /// <returns><see cref="MessageResponse"/></returns>  
    public async Task<MessageResponse> UpdateProjectMemberScopeAsync(string projectId, string memberId, UpdateProjectMemberScopeSchema updateProjectMemberScopeSchema) =>
        await PutAsync<MessageResponse>(
            $"{_urlPrefix}/{projectId}/{Constants.MEMBERS}/{memberId}/{Constants.SCOPES}",
            RequestContentUtil.CreatePayload(updateProjectMemberScopeSchema));

    /// <summary>
    /// Remove member from project, there is no response
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="memberId">Id of member</param>   
    public void RemoveProjectMember(string projectId, string memberId) =>
        _ = Delete($"{_urlPrefix}/{projectId}/{Constants.MEMBERS}/{memberId}");
    #endregion

    #region Usage

    /// <summary>
    /// Get usage request  associated with the project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="getProjectUsageRequestsSchema">Project usage request options<see cref="GetProjectUsageRequestsSchema"/>  </param>
    /// <returns><see cref="GetProjectUsageRequestsResponse"/></returns>
    public async Task<GetProjectUsageRequestsResponse> GetProjectsUsageRequestsAsync(string projectId, GetProjectUsageRequestsSchema getProjectUsageRequestsSchema)
    {
        var stringedOptions = QueryParameterUtil.GetParameters(getProjectUsageRequestsSchema);
        return await GetAsync<GetProjectUsageRequestsResponse>($"{_urlPrefix}/{projectId}/{Constants.REQUESTS}?{stringedOptions}");
    }

    /// <summary>
    /// Get the details associated with the requestID
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="requestId">Id of request</param>
    /// <returns><see cref="GetProjectUsageRequestResponse"/></returns>
    public async Task<GetProjectUsageRequestResponse> GetProjectUsageRequestAsync(string projectId, string requestId) =>
        await GetAsync<GetProjectUsageRequestResponse>($"{_urlPrefix}/{projectId}/{Constants.REQUESTS}/{requestId}");

    /// <summary>
    /// Gets a summary of usage
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="getProjectUsageSummarySchema">Usage summary options<see cref="GetProjectsUsageSummarySchema"/> </param>
    /// <returns><see cref="GetProjectUsageSummaryResponse"/></returns>
    public async Task<GetProjectUsageSummaryResponse> GetProjectUsageSummaryAsync(string projectId, GetProjectsUsageSummarySchema getProjectUsageSummarySchema)
    {
        var stringedOptions = QueryParameterUtil.GetParameters(getProjectUsageSummarySchema);
        return await GetAsync<GetProjectUsageSummaryResponse>(
            $"{_urlPrefix}/{projectId}/{Constants.USAGE}?{stringedOptions}");
    }

    /// <summary>
    /// Get usage fields 
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="getProjectUsageFieldsSchema">Project usage request field options<see cref="GetProjectUsageFieldsSchema"/></param>
    /// <returns><see cref="GetProjectUsageFieldsResponse"/></returns>
    public async Task<GetProjectUsageFieldsResponse> GetProjectUsageFieldsAsync(string projectId, GetProjectUsageFieldsSchema getProjectUsageFieldsSchema)
    {
        var stringedOptions = QueryParameterUtil.GetParameters(getProjectUsageFieldsSchema);
        return await GetAsync<GetProjectUsageFieldsResponse>(
            $"{_urlPrefix}/{projectId}/{Constants.USAGE}/fields?{stringedOptions}");
    }
    #endregion

    #region Balances

    /// <summary>
    /// Gets a list of balances
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="GetProjectBalancesResponse"/></returns>
    public async Task<GetProjectBalancesResponse> GetProjectBalancesAsync(string projectId) =>
        await GetAsync<GetProjectBalancesResponse>($"{_urlPrefix}/{projectId}/{Constants.BALANCES}");

    /// <summary>
    /// Get the balance details associated with the balance id
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="balanceId">Id of balance</param>
    /// <returns><see cref="GetProjectBalanceResponse"/></returns>
    public async Task<GetProjectBalanceResponse> GetProjectBalanceAsync(string projectId, string balanceId) =>
        await GetAsync<GetProjectBalanceResponse>($"{_urlPrefix}/{projectId}/{Constants.BALANCES}/{balanceId}");
    #endregion
}
