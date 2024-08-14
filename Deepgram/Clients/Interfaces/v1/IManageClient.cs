// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Manage.v1;

namespace Deepgram.Clients.Interfaces.v1;

/// <summary>
/// Implements version 1 of the Manage Client.
/// </summary>
/// <param name="apiKey">Required DeepgramApiKey</param>
/// <param name="deepgramClientOptions"><see cref="DeepgramHttpClientOptions"/> for HttpClient Configuration</param>
public interface IManageClient
{
    #region Projects
    /// <summary>
    /// Gets projects associated to ApiKey 
    /// </summary>
    /// <returns><see cref="ProjectsResponse"/></returns>
    public Task<ProjectsResponse> GetProjects(CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Gets project associated with project Id
    /// </summary>
    /// <param name="projectId">Id of Project</param>
    /// <returns><see cref="ProjectResponse"/></returns>
    public Task<ProjectResponse> GetProject(string projectId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Update a project associated with the projectID
    /// </summary>
    /// <param name="projectId">ID of project</param>
    /// <param name="updateProjectSchema"><see cref="ProjectSchema"/> for project</param>
    /// <returns><see cref="MessageResponse"/></returns>
    // USES PATCH
    public Task<MessageResponse> UpdateProject(string projectId, ProjectSchema updateProjectSchema, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Deletes a project, no response will be returned
    /// </summary>
    /// <param name="projectId">Id of project</param>
    // No response expected
    public Task<MessageResponse> DeleteProject(string projectId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// leave project associated with the project Id
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="MessageResponse"/></returns>
    public Task<MessageResponse> LeaveProject(string projectId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Get all models associated with the project Id
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="ModelsResponse"/></returns>
    public Task<ModelsResponse> GetProjectModels(string projectId, ModelSchema? modelSchema = null, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Get a specific model associated with the project Id
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="modelId">Id of model</param>
    /// <returns><see cref="ModelResponse"/></returns>
    public Task<ModelResponse> GetProjectModel(string projectId, string modelId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);
    #endregion

    #region Models
    /// <summary>
    /// Gets models available in Deepgram
    /// </summary>
    /// <returns><see cref="ModelsResponse"/></returns>
    public Task<ModelsResponse> GetModels(ModelSchema? modelSchema = null, CancellationTokenSource? cancellationToken = default,
    Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Gets a specific model within Deepgram
    /// </summary>
    /// <param name="projectId">Id of Model</param>
    /// <returns><see cref="ModelResponse"/></returns>
    public Task<ModelResponse> GetModel(string modelId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);
    #endregion

    #region ProjectKeys
    /// <summary>
    /// Get the keys associated with the project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="KeysResponse"/></returns>
    public Task<KeysResponse> GetKeys(string projectId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Get details of key associated with the key ID
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="keyId">Id of key</param>
    /// <returns><see cref="KeyScopeResponse"/></returns>
    public Task<KeyScopeResponse> GetKey(string projectId, string keyId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Create a key in the associated project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="createProjectKeySchema"><see cref="KeySchema"/> for the key to be created</param>
    /// <returns><see cref="KeyResponse"/></returns>
    public Task<KeyResponse> CreateKey(string projectId, KeySchema keySchema, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Remove key from project, No response returned
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="keyId">Id of key</param>
    // Nothing being returned
    public Task<MessageResponse> DeleteKey(string projectId, string keyId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);
    #endregion

    #region ProjectInvites
    /// <summary>
    /// Get any invites that are associated with project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="InvitesResponse"/></returns>
    public Task<InvitesResponse> GetInvites(string projectId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Delete a project invite that has been sent
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="email">email of the invite to be removed</param>
    //no response expected
    public Task<MessageResponse> DeleteInvite(string projectId, string email, CancellationTokenSource? cancellationToken = default,
    Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Send a invite to the associated project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="inviteSchema"><see cref="InviteSchema"/> for a invite to project</param>
    /// <returns><see cref="MessageResponse"/></returns>
    public Task<MessageResponse> SendInvite(string projectId, InviteSchema inviteSchema, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);
    #endregion

    #region Members
    /// <summary>
    /// Get the members associated with the project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="MembersResponse"/></returns>
    public Task<MembersResponse> GetMembers(string projectId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Get the scopes associated with member
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="memberId">Id of member</param>
    /// <returns><see cref="MemberScopesResponse"/></returns>
    public Task<MemberScopesResponse> GetMemberScopes(string projectId, string memberId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Update the scopes fot the member
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="memberId">Id of member</param>
    /// <param name="memberScopeSchema">updates scope options for member<see cref="MemberScopeSchema"/></param>
    /// <returns><see cref="MessageResponse"/></returns>  
    public Task<MessageResponse> UpdateMemberScope(string projectId, string memberId, MemberScopeSchema scopeSchema,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null);

    /// <summary>
    /// Remove member from project, there is no response
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="memberId">Id of member</param>   
    //No response expected
    public Task<MessageResponse> RemoveMember(string projectId, string memberId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);
    #endregion

    #region Usage
    /// <summary>
    /// Get usage request  associated with the project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="UsageRequestsSchema">Project usage request options<see cref="UsageRequestsSchema"/>  </param>
    /// <returns><see cref="UsageRequestsResponse"/></returns>
    public Task<UsageRequestsResponse> GetUsageRequests(string projectId, UsageRequestsSchema usageRequestsSchema,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Get the details associated with the requestID
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="requestId">Id of request</param>
    /// <returns><see cref="UsageRequestResponse"/></returns>
    public Task<UsageRequestResponse> GetUsageRequest(string projectId, string requestId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Gets a summary of usage
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="getUsageSummarySchema">Usage summary options<see cref="UsageSummarySchema"/> </param>
    /// <returns><see cref="UsageSummaryResponse"/></returns>
    public Task<UsageSummaryResponse> GetUsageSummary(string projectId, UsageSummarySchema summarySchema,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Get usage fields 
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="getUsageFieldsSchema">Project usage request field options<see cref="UsageFieldsSchema"/></param>
    /// <returns><see cref="UsageFieldsResponse"/></returns>
    public Task<UsageFieldsResponse> GetUsageFields(string projectId, UsageFieldsSchema fieldsSchema,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);
    #endregion

    #region Balances
    /// <summary>
    /// Gets a list of balances
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="BalancesResponse"/></returns>
    public Task<BalancesResponse> GetBalances(string projectId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Get the balance details associated with the balance id
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="balanceId">Id of balance</param>
    /// <returns><see cref="BalanceResponse"/></returns>
    public Task<BalanceResponse> GetBalance(string projectId, string balanceId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);
    #endregion
}
