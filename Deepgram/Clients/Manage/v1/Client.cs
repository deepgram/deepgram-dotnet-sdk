// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Manage.v1;

namespace Deepgram.Clients.Manage.v1;

/// <summary>
/// Implements version 1 of the Manage Client.
/// </summary>
/// <param name="apiKey">Required DeepgramApiKey</param>
/// <param name="deepgramClientOptions"><see cref="DeepgramClientOptions"/> for HttpClient Configuration</param>
public class Client(string apiKey, DeepgramClientOptions? deepgramClientOptions = null)
    : AbstractRestClient(apiKey, deepgramClientOptions)
{
    #region Projects
    /// <summary>
    /// Gets projects associated to ApiKey 
    /// </summary>
    /// <returns><see cref="ProjectsResponse"/></returns>
    public async Task<ProjectsResponse> GetProjects(CancellationToken cancellationToken = default, Dictionary<string, string>? addons = null) =>
        await GetAsync<ProjectsResponse>(UriSegments.PROJECTS, cancellationToken, addons);

    /// <summary>
    /// Gets project associated with project Id
    /// </summary>
    /// <param name="projectId">Id of Project</param>
    /// <returns><see cref="ProjectResponse"/></returns>
    public async Task<ProjectResponse> GetProject(string projectId, CancellationToken cancellationToken = default, Dictionary<string, string>? addons = null) =>
        await GetAsync<ProjectResponse>($"{UriSegments.PROJECTS}/{projectId}", cancellationToken, addons);

    /// <summary>
    /// Update a project associated with the projectID
    /// </summary>
    /// <param name="projectId">ID of project</param>
    /// <param name="updateProjectSchema"><see cref="ProjectSchema"/> for project</param>
    /// <returns><see cref="MessageResponse"/></returns>
    // USES PATCH
    public async Task<MessageResponse> UpdateProject(string projectId, ProjectSchema updateProjectSchema, CancellationToken cancellationToken = default, Dictionary<string, string>? addons = null) =>
        await PatchAsync<MessageResponse>(
            $"{UriSegments.PROJECTS}/{projectId}",
            RequestContentUtil.CreatePayload(updateProjectSchema), cancellationToken, addons);

    /// <summary>
    /// Deletes a project, no response will be returned
    /// </summary>
    /// <param name="projectId">Id of project</param>
    // No response expected
    public async Task DeleteProject(string projectId, CancellationToken cancellationToken = default, Dictionary<string, string>? addons = null) =>
     await DeleteAsync($"{UriSegments.PROJECTS}/{projectId}", cancellationToken, addons);

    /// <summary>
    /// leave project associated with the project Id
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="MessageResponse"/></returns>
    public async Task<MessageResponse> LeaveProject(string projectId, CancellationToken cancellationToken = default, Dictionary<string, string>? addons = null) =>
        await DeleteAsync<MessageResponse>($"{UriSegments.PROJECTS}/{projectId}/leave", cancellationToken, addons);

    #endregion

    #region ProjectKeys

    /// <summary>
    /// Get the keys associated with the project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="KeysResponse"/></returns>
    public async Task<KeysResponse> GetKeys(string projectId, CancellationToken cancellationToken = default, Dictionary<string, string>? addons = null) =>
        await GetAsync<KeysResponse>($"{UriSegments.PROJECTS}/{projectId}/{UriSegments.KEYS}", cancellationToken, addons);

    /// <summary>
    /// Get details of key associated with the key ID
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="keyId">Id of key</param>
    /// <returns><see cref="KeyScopeResponse"/></returns>
    public async Task<KeyScopeResponse> GetKey(string projectId, string keyId, CancellationToken cancellationToken = default, Dictionary<string, string>? addons = null) =>
        await GetAsync<KeyScopeResponse>($"{UriSegments.PROJECTS}/{projectId}/{UriSegments.KEYS}/{keyId}", cancellationToken, addons);

    /// <summary>
    /// Create a key in the associated project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="createProjectKeySchema"><see cref="KeySchema"/> for the key to be created</param>
    /// <returns><see cref="KeyResponse"/></returns>
    public async Task<KeyResponse> CreateKey(string projectId, KeySchema createProjectKeySchema, CancellationToken cancellationToken = default, Dictionary<string, string>? addons = null)
    {
        // TODO: think about logging here based on coderabbit feedback
        if (createProjectKeySchema.ExpirationDate is not null && createProjectKeySchema.TimeToLiveInSeconds is not null)
        {
            Log.CreateKeyError(_logger, createProjectKeySchema);
            throw new ArgumentException("Both ExpirationDate and TimeToLiveInSeconds is set. set either one but not both");
        }

        return await PostAsync<KeyResponse>(
                     $"{UriSegments.PROJECTS}/{projectId}/keys",
                     RequestContentUtil.CreatePayload(createProjectKeySchema), cancellationToken, addons);
    }


    /// <summary>
    /// Remove key from project, No response returned
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="keyId">Id of key</param>
    // Nothing being returned
    public async Task DeleteKey(string projectId, string keyId, CancellationToken cancellationToken = default, Dictionary<string, string>? addons = null) =>
        await DeleteAsync($"{UriSegments.PROJECTS}/{projectId}/keys/{keyId}", cancellationToken, addons);

    #endregion

    #region ProjectInvites
    /// <summary>
    /// Get any invites that are associated with project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="InvitesResponse"/></returns>
    public async Task<InvitesResponse> GetInvites(string projectId, CancellationToken cancellationToken = default, Dictionary<string, string>? addons = null) =>
        await GetAsync<InvitesResponse>($"{UriSegments.PROJECTS}/{projectId}/{UriSegments.INVITES}", cancellationToken, addons);

    /// <summary>
    /// Delete a project invite that has been sent
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="email">email of the invite to be removed</param>
    //no response expected
    public async Task DeleteInvite(string projectId, string email, CancellationToken cancellationToken = default, Dictionary<string, string>? addons = null) =>
        await DeleteAsync($"{UriSegments.PROJECTS}/{projectId}/{UriSegments.INVITES}/{email}", cancellationToken, addons);

    /// <summary>
    /// Send a invite to the associated project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="inviteSchema"><see cref="InviteSchema"/> for a invite to project</param>
    /// <returns><see cref="MessageResponse"/></returns>
    public async Task<MessageResponse> SendInvite(string projectId, InviteSchema inviteSchema, CancellationToken cancellationToken = default, Dictionary<string, string>? addons = null) =>
        await PostAsync<MessageResponse>(
            $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.INVITES}",
            RequestContentUtil.CreatePayload(inviteSchema), cancellationToken, addons);
    #endregion

    #region Members
    /// <summary>
    /// Get the members associated with the project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="MembersResponse"/></returns>
    public async Task<MembersResponse> GetMembers(string projectId, CancellationToken cancellationToken = default, Dictionary<string, string>? addons = null) =>
        await GetAsync<MembersResponse>($"{UriSegments.PROJECTS}/{projectId}/{UriSegments.MEMBERS}", cancellationToken, addons);

    /// <summary>
    /// Get the scopes associated with member
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="memberId">Id of member</param>
    /// <returns><see cref="MemberScopesResponse"/></returns>
    public async Task<MemberScopesResponse> GetMemberScopes(string projectId, string memberId, CancellationToken cancellationToken = default, Dictionary<string, string>? addons = null) =>
        await GetAsync<MemberScopesResponse>($"{UriSegments.PROJECTS}/{projectId}/{UriSegments.MEMBERS}/{memberId}/{UriSegments.SCOPES}", cancellationToken, addons);

    /// <summary>
    /// Update the scopes fot the member
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="memberId">Id of member</param>
    /// <param name="memberScopeSchema">updates scope options for member<see cref="MemberScopeSchema"/></param>
    /// <returns><see cref="MessageResponse"/></returns>  
    public async Task<MessageResponse> UpdateMemberScope(string projectId, string memberId, MemberScopeSchema memberScopeSchema, CancellationToken cancellationToken = default, Dictionary<string, string>? addons = null) =>
        await PutAsync<MessageResponse>(
            $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.MEMBERS}/{memberId}/{UriSegments.SCOPES}",
            RequestContentUtil.CreatePayload(memberScopeSchema), cancellationToken, addons);

    /// <summary>
    /// Remove member from project, there is no response
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="memberId">Id of member</param>   
    //No response expected
    public async Task RemoveMember(string projectId, string memberId, CancellationToken cancellationToken = default, Dictionary<string, string>? addons = null) =>
        await DeleteAsync($"{UriSegments.PROJECTS}/{projectId}/{UriSegments.MEMBERS}/{memberId}", cancellationToken, addons);
    #endregion

    #region Usage

    /// <summary>
    /// Get usage request  associated with the project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="UsageRequestsSchema">Project usage request options<see cref="UsageRequestsSchema"/>  </param>
    /// <returns><see cref="UsageRequestsResponse"/></returns>
    public async Task<UsageRequestsResponse> GetUsageRequests(string projectId, UsageRequestsSchema UsageRequestsSchema, CancellationToken cancellationToken = default, Dictionary<string, string>? addons = null)
    {
        var stringedOptions = QueryParameterUtil.GetParameters(UsageRequestsSchema);
        return await GetAsync<UsageRequestsResponse>($"{UriSegments.PROJECTS}/{projectId}/{UriSegments.REQUESTS}?{stringedOptions}", cancellationToken, addons);
    }

    /// <summary>
    /// Get the details associated with the requestID
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="requestId">Id of request</param>
    /// <returns><see cref="UsageRequestResponse"/></returns>
    public async Task<UsageRequestResponse> GetUsageRequest(string projectId, string requestId, CancellationToken cancellationToken = default, Dictionary<string, string>? addons = null) =>
        await GetAsync<UsageRequestResponse>($"{UriSegments.PROJECTS}/{projectId}/{UriSegments.REQUESTS}/{requestId}", cancellationToken, addons);

    /// <summary>
    /// Gets a summary of usage
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="getProjectUsageSummarySchema">Usage summary options<see cref="UsageSummarySchema"/> </param>
    /// <returns><see cref="UsageSummaryResponse"/></returns>
    public async Task<UsageSummaryResponse> GetUsageSummary(string projectId, UsageSummarySchema getProjectUsageSummarySchema, CancellationToken cancellationToken = default, Dictionary<string, string>? addons = null)
    {
        var stringedOptions = QueryParameterUtil.GetParameters(getProjectUsageSummarySchema);
        return await GetAsync<UsageSummaryResponse>(
            $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.USAGE}?{stringedOptions}", cancellationToken, addons);
    }

    /// <summary>
    /// Get usage fields 
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="getProjectUsageFieldsSchema">Project usage request field options<see cref="UsageFieldsSchema"/></param>
    /// <returns><see cref="UsageFieldsResponse"/></returns>
    public async Task<UsageFieldsResponse> GetUsageFields(string projectId, UsageFieldsSchema getProjectUsageFieldsSchema, CancellationToken cancellationToken = default, Dictionary<string, string>? addons = null)
    {
        var stringedOptions = QueryParameterUtil.GetParameters(getProjectUsageFieldsSchema);
        return await GetAsync<UsageFieldsResponse>(
            $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.USAGE}/fields?{stringedOptions}", cancellationToken, addons);
    }
    #endregion

    #region Balances

    /// <summary>
    /// Gets a list of balances
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="BalancesResponse"/></returns>
    public async Task<BalancesResponse> GetBalances(string projectId, CancellationToken cancellationToken = default, Dictionary<string, string>? addons = null) =>
        await GetAsync<BalancesResponse>($"{UriSegments.PROJECTS}/{projectId}/{UriSegments.BALANCES}", cancellationToken, addons);

    /// <summary>
    /// Get the balance details associated with the balance id
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="balanceId">Id of balance</param>
    /// <returns><see cref="BalanceResponse"/></returns>
    public async Task<BalanceResponse> GetBalance(string projectId, string balanceId, CancellationToken cancellationToken = default, Dictionary<string, string>? addons = null) =>
        await GetAsync<BalanceResponse>($"{UriSegments.PROJECTS}/{projectId}/{UriSegments.BALANCES}/{balanceId}", cancellationToken, addons);
    #endregion
}
