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
/// <param name="deepgramClientOptions"><see cref="DeepgramHttpClientOptions"/> for HttpClient Configuration</param>
public class Client(string? apiKey = null, DeepgramHttpClientOptions? deepgramClientOptions = null)
    : AbstractRestClient(apiKey, deepgramClientOptions)
{
    #region Projects
    /// <summary>
    /// Gets projects associated to ApiKey 
    /// </summary>
    /// <returns><see cref="ProjectsResponse"/></returns>
    public async Task<ProjectsResponse> GetProjects(CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null) =>
        await GetAsync<ProjectsResponse>(GetUri(_options, $"{UriSegments.PROJECTS}"), cancellationToken, addons, headers);

    /// <summary>
    /// Gets project associated with project Id
    /// </summary>
    /// <param name="projectId">Id of Project</param>
    /// <returns><see cref="ProjectResponse"/></returns>
    public async Task<ProjectResponse> GetProject(string projectId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null) =>
        await GetAsync<ProjectResponse>(
            GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}"), cancellationToken, addons, headers
            );

    /// <summary>
    /// Update a project associated with the projectID
    /// </summary>
    /// <param name="projectId">ID of project</param>
    /// <param name="updateProjectSchema"><see cref="ProjectSchema"/> for project</param>
    /// <returns><see cref="MessageResponse"/></returns>
    // USES PATCH
    public async Task<MessageResponse> UpdateProject(string projectId, ProjectSchema updateProjectSchema, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null) =>
        await PatchAsync<ProjectSchema, MessageResponse>(
            GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}"), updateProjectSchema, cancellationToken, addons, headers
            );

    /// <summary>
    /// Deletes a project, no response will be returned
    /// </summary>
    /// <param name="projectId">Id of project</param>
    // No response expected
    public async Task<MessageResponse> DeleteProject(string projectId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null) =>
     await DeleteAsync<MessageResponse>(
         GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}"), cancellationToken, addons, headers
         );

    /// <summary>
    /// leave project associated with the project Id
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="MessageResponse"/></returns>
    public async Task<MessageResponse> LeaveProject(string projectId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null) =>
        await DeleteAsync<MessageResponse>(
            GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/leave"), cancellationToken, addons, headers
            );
    #endregion

    #region ProjectKeys

    /// <summary>
    /// Get the keys associated with the project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="KeysResponse"/></returns>
    public async Task<KeysResponse> GetKeys(string projectId, CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null) =>
        await GetAsync<KeysResponse>(
            GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.KEYS}"), cancellationToken, addons, headers
            );

    /// <summary>
    /// Get details of key associated with the key ID
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="keyId">Id of key</param>
    /// <returns><see cref="KeyScopeResponse"/></returns>
    public async Task<KeyScopeResponse> GetKey(string projectId, string keyId, CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null) =>
        await GetAsync<KeyScopeResponse>(
            GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.KEYS}/{keyId}"), cancellationToken, addons, headers
            );

    /// <summary>
    /// Create a key in the associated project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="createProjectKeySchema"><see cref="KeySchema"/> for the key to be created</param>
    /// <returns><see cref="KeyResponse"/></returns>
    public async Task<KeyResponse> CreateKey(string projectId, KeySchema createKeySchema, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        // TODO: think about logging here based on coderabbit feedback
        if (createKeySchema.ExpirationDate is not null && createKeySchema.TimeToLiveInSeconds is not null)
        {
            Log.CreateKeyError(_logger, createKeySchema);
            throw new ArgumentException("Both ExpirationDate and TimeToLiveInSeconds is set. set either one but not both");
        }

        return await PostAsync<KeySchema, KeyResponse>(
            GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/keys"), createKeySchema, cancellationToken, addons, headers
            );
    }


    /// <summary>
    /// Remove key from project, No response returned
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="keyId">Id of key</param>
    // Nothing being returned
    public async Task DeleteKey(string projectId, string keyId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null) =>
        await DeleteAsync<MessageResponse>(GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/keys/{keyId}"), cancellationToken, addons, headers);
    #endregion

    #region ProjectInvites
    /// <summary>
    /// Get any invites that are associated with project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="InvitesResponse"/></returns>
    public async Task<InvitesResponse> GetInvites(string projectId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null) =>
        await GetAsync<InvitesResponse>(GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.INVITES}"), cancellationToken, addons, headers);

    /// <summary>
    /// Delete a project invite that has been sent
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="email">email of the invite to be removed</param>
    //no response expected
    public async Task DeleteInvite(string projectId, string email, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null) =>
        await DeleteAsync<MessageResponse>(GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.INVITES}/{email}"),
            cancellationToken, addons, headers);

    /// <summary>
    /// Send a invite to the associated project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="inviteSchema"><see cref="InviteSchema"/> for a invite to project</param>
    /// <returns><see cref="MessageResponse"/></returns>
    public async Task<MessageResponse> SendInvite(string projectId, InviteSchema inviteSchema, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null) =>
        await PostAsync<InviteSchema, MessageResponse>(
            GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.INVITES}"), inviteSchema, cancellationToken, addons, headers
            );
    #endregion

    #region Members
    /// <summary>
    /// Get the members associated with the project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="MembersResponse"/></returns>
    public async Task<MembersResponse> GetMembers(string projectId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null) =>
        await GetAsync<MembersResponse>(GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.MEMBERS}"), cancellationToken, addons, headers);

    /// <summary>
    /// Get the scopes associated with member
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="memberId">Id of member</param>
    /// <returns><see cref="MemberScopesResponse"/></returns>
    public async Task<MemberScopesResponse> GetMemberScopes(string projectId, string memberId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null) =>
        await GetAsync<MemberScopesResponse>(GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.MEMBERS}/{memberId}/{UriSegments.SCOPES}"),
            cancellationToken, addons, headers
            );

    /// <summary>
    /// Update the scopes fot the member
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="memberId">Id of member</param>
    /// <param name="memberScopeSchema">updates scope options for member<see cref="MemberScopeSchema"/></param>
    /// <returns><see cref="MessageResponse"/></returns>  
    public async Task<MessageResponse> UpdateMemberScope(string projectId, string memberId, MemberScopeSchema memberScopeSchema,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null) =>
        await PutAsync<MemberScopeSchema, MessageResponse>(
            GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.MEMBERS}/{memberId}/{UriSegments.SCOPES}"), memberScopeSchema,
            cancellationToken, addons, headers
            );

    /// <summary>
    /// Remove member from project, there is no response
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="memberId">Id of member</param>   
    //No response expected
    public async Task RemoveMember(string projectId, string memberId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null) =>
        await DeleteAsync<MessageResponse>(GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.MEMBERS}/{memberId}"),
            cancellationToken, addons, headers
            );
    #endregion

    #region Usage

    /// <summary>
    /// Get usage request  associated with the project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="UsageRequestsSchema">Project usage request options<see cref="UsageRequestsSchema"/>  </param>
    /// <returns><see cref="UsageRequestsResponse"/></returns>
    public async Task<UsageRequestsResponse> GetUsageRequests(string projectId, UsageRequestsSchema usageRequestsSchema,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        return await GetAsync<UsageRequestsSchema, UsageRequestsResponse>(GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.REQUESTS}"),
            usageRequestsSchema, cancellationToken, addons, headers
            );
    }

    /// <summary>
    /// Get the details associated with the requestID
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="requestId">Id of request</param>
    /// <returns><see cref="UsageRequestResponse"/></returns>
    public async Task<UsageRequestResponse> GetUsageRequest(string projectId, string requestId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null) =>
        await GetAsync<UsageRequestResponse>(GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.REQUESTS}/{requestId}"),
            cancellationToken, addons, headers
            );

    /// <summary>
    /// Gets a summary of usage
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="getUsageSummarySchema">Usage summary options<see cref="UsageSummarySchema"/> </param>
    /// <returns><see cref="UsageSummaryResponse"/></returns>
    public async Task<UsageSummaryResponse> GetUsageSummary(string projectId, UsageSummarySchema usageSummarySchema,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        return await GetAsync<UsageSummarySchema, UsageSummaryResponse>(
            GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.USAGE}"), usageSummarySchema, cancellationToken, addons, headers
            );
    }

    /// <summary>
    /// Get usage fields 
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="getUsageFieldsSchema">Project usage request field options<see cref="UsageFieldsSchema"/></param>
    /// <returns><see cref="UsageFieldsResponse"/></returns>
    public async Task<UsageFieldsResponse> GetUsageFields(string projectId, UsageFieldsSchema usageFieldsSchema,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        return await GetAsync<UsageFieldsSchema, UsageFieldsResponse>(
            GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.USAGE}/fields"), usageFieldsSchema, cancellationToken, addons, headers);
    }
    #endregion

    #region Balances
    /// <summary>
    /// Gets a list of balances
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="BalancesResponse"/></returns>
    public async Task<BalancesResponse> GetBalances(string projectId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null) =>
        await GetAsync<BalancesResponse>(GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.BALANCES}"), cancellationToken, addons, headers);

    /// <summary>
    /// Get the balance details associated with the balance id
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="balanceId">Id of balance</param>
    /// <returns><see cref="BalanceResponse"/></returns>
    public async Task<BalanceResponse> GetBalance(string projectId, string balanceId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null) =>
        await GetAsync<BalanceResponse>(GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.BALANCES}/{balanceId}"), cancellationToken, addons, headers);
    #endregion
}
