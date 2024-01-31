using Deepgram.Models.Manage;
using Deepgram.Models.Manage.v1;
using Deepgram.Models.Shared.v1;

namespace Deepgram;

/// <summary>
///  Client containing methods for interacting with API's to manage project(s)
/// </summary>
/// <param name="apiKey">Required DeepgramApiKey</param>
/// <param name="deepgramClientOptions"><see cref="DeepgramClientOptions"/> for HttpClient Configuration</param>
public class ManageClient(string apiKey, DeepgramClientOptions? deepgramClientOptions = null)
    : AbstractRestClient(apiKey, deepgramClientOptions)
{
    #region Projects
    /// <summary>
    /// Gets projects associated to ApiKey 
    /// </summary>
    /// <returns><see cref="GetProjectsResponse"/></returns>
    public async Task<GetProjectsResponse> GetProjects() =>
        await GetAsync<GetProjectsResponse>(UriSegments.PROJECTS);

    /// <summary>
    /// Gets project associated with project Id
    /// </summary>
    /// <param name="projectId">Id of Project</param>
    /// <returns><see cref="GetProjectResponse"/></returns>
    public async Task<GetProjectResponse> GetProject(string projectId) =>
        await GetAsync<GetProjectResponse>($"{UriSegments.PROJECTS}/{projectId}");

    /// <summary>
    /// Update a project associated with the projectID
    /// </summary>
    /// <param name="projectId">ID of project</param>
    /// <param name="updateProjectSchema"><see cref="UpdateProjectSchema"/> for project</param>
    /// <returns><see cref="MessageResponse"/></returns>
    // USES PATCH
    public async Task<MessageResponse> UpdateProject(string projectId, UpdateProjectSchema updateProjectSchema) =>
        await PatchAsync<MessageResponse>(
            $"{UriSegments.PROJECTS}/{projectId}",
            RequestContentUtil.CreatePayload(updateProjectSchema));

    /// <summary>
    /// Deletes a project, no response will be returned
    /// </summary>
    /// <param name="projectId">Id of project</param>
    // No response expected
    public async Task DeleteProject(string projectId) =>
     await DeleteAsync($"{UriSegments.PROJECTS}/{projectId}");

    /// <summary>
    /// leave project associated with the project Id
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="MessageResponse"/></returns>
    public async Task<MessageResponse> LeaveProject(string projectId) =>
        await DeleteAsync<MessageResponse>($"{UriSegments.PROJECTS}/{projectId}/leave");

    #endregion

    #region ProjectKeys

    /// <summary>
    /// Get the keys associated with the project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="GetProjectKeysResponse"/></returns>
    public async Task<GetProjectKeysResponse> GetProjectKeys(string projectId) =>
        await GetAsync<GetProjectKeysResponse>($"{UriSegments.PROJECTS}/{projectId}/{UriSegments.KEYS}");

    /// <summary>
    /// Get details of key associated with the key ID
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="keyId">Id of key</param>
    /// <returns><see cref="GetProjectKeyResponse"/></returns>
    public async Task<GetProjectKeyResponse> GetProjectKey(string projectId, string keyId) =>
        await GetAsync<GetProjectKeyResponse>($"{UriSegments.PROJECTS}/{projectId}/{UriSegments.KEYS}/{keyId}");

    /// <summary>
    /// Create a key in the associated project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="createProjectKeySchema"><see cref="CreateProjectKeySchema"/> for the key to be created</param>
    /// <returns><see cref="CreateProjectKeyResponse"/></returns>
    public async Task<CreateProjectKeyResponse> CreateProjectKey(string projectId, CreateProjectKeySchema createProjectKeySchema)
    {
        if (createProjectKeySchema.ExpirationDate is not null && createProjectKeySchema.TimeToLiveInSeconds is not null)
        {
            Log.CreateProjectKeyError(_logger, createProjectKeySchema);
            throw new ArgumentException("Both ExpirationDate and TimeToLiveInSeconds is set. set either one but not both");
        }

        return await PostAsync<CreateProjectKeyResponse>(
                     $"{UriSegments.PROJECTS}/{projectId}/keys",
                     RequestContentUtil.CreatePayload(createProjectKeySchema));
    }


    /// <summary>
    /// Remove key from project, No response returned
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="keyId">Id of key</param>
    // Nothing being returned
    public async Task DeleteProjectKey(string projectId, string keyId) =>
        await DeleteAsync($"{UriSegments.PROJECTS}/{projectId}/keys/{keyId}");

    #endregion

    #region ProjectInvites
    /// <summary>
    /// Get any invites that are associated with project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="GetProjectInvitesResponse"/></returns>
    public async Task<GetProjectInvitesResponse> GetProjectInvites(string projectId) =>
        await GetAsync<GetProjectInvitesResponse>($"{UriSegments.PROJECTS}/{projectId}/{UriSegments.INVITES}");

    /// <summary>
    /// Delete a project invite that has been sent
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="email">email of the invite to be removed</param>
    //no response expected
    public async Task DeleteProjectInvite(string projectId, string email) =>
        await DeleteAsync($"{UriSegments.PROJECTS}/{projectId}/{UriSegments.INVITES}/{email}");

    /// <summary>
    /// Send a invite to the associated project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="sendProjectInviteSchema"><see cref="SendProjectInviteSchema"/> for a invite to project</param>
    /// <returns><see cref="MessageResponse"/></returns>
    public async Task<MessageResponse> SendProjectInvite(string projectId, SendProjectInviteSchema sendProjectInviteSchema) =>
        await PostAsync<MessageResponse>(
            $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.INVITES}",
            RequestContentUtil.CreatePayload(sendProjectInviteSchema));
    #endregion

    #region Members
    /// <summary>
    /// Get the members associated with the project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="GetProjectMembersResponse"/></returns>
    public async Task<GetProjectMembersResponse> GetProjectMembers(string projectId) =>
        await GetAsync<GetProjectMembersResponse>($"{UriSegments.PROJECTS}/{projectId}/{UriSegments.MEMBERS}");

    /// <summary>
    /// Get the scopes associated with member
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="memberId">Id of member</param>
    /// <returns><see cref="GetProjectMemberScopesResponse"/></returns>
    public async Task<GetProjectMemberScopesResponse> GetProjectMemberScopes(string projectId, string memberId) =>
        await GetAsync<GetProjectMemberScopesResponse>($"{UriSegments.PROJECTS}/{projectId}/{UriSegments.MEMBERS}/{memberId}/{UriSegments.SCOPES}");

    /// <summary>
    /// Update the scopes fot the member
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="memberId">Id of member</param>
    /// <param name="updateProjectMemberScopeSchema">updates scope options for member<see cref="UpdateProjectMemberScopeSchema"/></param>
    /// <returns><see cref="MessageResponse"/></returns>  
    public async Task<MessageResponse> UpdateProjectMemberScope(string projectId, string memberId, UpdateProjectMemberScopeSchema updateProjectMemberScopeSchema) =>
        await PutAsync<MessageResponse>(
            $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.MEMBERS}/{memberId}/{UriSegments.SCOPES}",
            RequestContentUtil.CreatePayload(updateProjectMemberScopeSchema));

    /// <summary>
    /// Remove member from project, there is no response
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="memberId">Id of member</param>   
    //No response expected
    public async Task RemoveProjectMember(string projectId, string memberId) =>
        await DeleteAsync($"{UriSegments.PROJECTS}/{projectId}/{UriSegments.MEMBERS}/{memberId}");
    #endregion

    #region Usage

    /// <summary>
    /// Get usage request  associated with the project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="getProjectUsageRequestsSchema">Project usage request options<see cref="GetProjectUsageRequestsSchema"/>  </param>
    /// <returns><see cref="GetProjectUsageRequestsResponse"/></returns>
    public async Task<GetProjectUsageRequestsResponse> GetProjectUsageRequests(string projectId, GetProjectUsageRequestsSchema getProjectUsageRequestsSchema)
    {
        var stringedOptions = QueryParameterUtil.GetParameters(getProjectUsageRequestsSchema);
        return await GetAsync<GetProjectUsageRequestsResponse>($"{UriSegments.PROJECTS}/{projectId}/{UriSegments.REQUESTS}?{stringedOptions}");
    }

    /// <summary>
    /// Get the details associated with the requestID
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="requestId">Id of request</param>
    /// <returns><see cref="GetProjectUsageRequestResponse"/></returns>
    public async Task<GetProjectUsageRequestResponse> GetProjectUsageRequest(string projectId, string requestId) =>
        await GetAsync<GetProjectUsageRequestResponse>($"{UriSegments.PROJECTS}/{projectId}/{UriSegments.REQUESTS}/{requestId}");

    /// <summary>
    /// Gets a summary of usage
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="getProjectUsageSummarySchema">Usage summary options<see cref="GetProjectsUsageSummarySchema"/> </param>
    /// <returns><see cref="GetProjectUsageSummaryResponse"/></returns>
    public async Task<GetProjectUsageSummaryResponse> GetProjectUsageSummary(string projectId, GetProjectsUsageSummarySchema getProjectUsageSummarySchema)
    {
        var stringedOptions = QueryParameterUtil.GetParameters(getProjectUsageSummarySchema);
        return await GetAsync<GetProjectUsageSummaryResponse>(
            $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.USAGE}?{stringedOptions}");
    }

    /// <summary>
    /// Get usage fields 
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="getProjectUsageFieldsSchema">Project usage request field options<see cref="GetProjectUsageFieldsSchema"/></param>
    /// <returns><see cref="GetProjectUsageFieldsResponse"/></returns>
    public async Task<GetProjectUsageFieldsResponse> GetProjectUsageFields(string projectId, GetProjectUsageFieldsSchema getProjectUsageFieldsSchema)
    {
        var stringedOptions = QueryParameterUtil.GetParameters(getProjectUsageFieldsSchema);
        return await GetAsync<GetProjectUsageFieldsResponse>(
            $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.USAGE}/fields?{stringedOptions}");
    }
    #endregion

    #region Balances

    /// <summary>
    /// Gets a list of balances
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="GetProjectBalancesResponse"/></returns>
    public async Task<GetProjectBalancesResponse> GetProjectBalances(string projectId) =>
        await GetAsync<GetProjectBalancesResponse>($"{UriSegments.PROJECTS}/{projectId}/{UriSegments.BALANCES}");

    /// <summary>
    /// Get the balance details associated with the balance id
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="balanceId">Id of balance</param>
    /// <returns><see cref="GetProjectBalanceResponse"/></returns>
    public async Task<GetProjectBalanceResponse> GetProjectBalance(string projectId, string balanceId) =>
        await GetAsync<GetProjectBalanceResponse>($"{UriSegments.PROJECTS}/{projectId}/{UriSegments.BALANCES}/{balanceId}");
    #endregion
}
