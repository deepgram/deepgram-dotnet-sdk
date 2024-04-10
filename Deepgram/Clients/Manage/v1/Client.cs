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
public class Client(string? apiKey = null, DeepgramHttpClientOptions? deepgramClientOptions = null, string? httpId = null)
    : AbstractRestClient(apiKey, deepgramClientOptions, httpId)
{
    #region Projects
    /// <summary>
    /// Gets projects associated to ApiKey 
    /// </summary>
    /// <returns><see cref="ProjectsResponse"/></returns>
    public async Task<ProjectsResponse> GetProjects(CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("ManageClient.GetProjects", "ENTER");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}");
        var result = await GetAsync<ProjectsResponse>(uri, cancellationToken, addons, headers);

        Log.Information("GetProjects", $"{uri} Succeeded");
        Log.Debug("GetProjects", $"result: {result}");
        Log.Verbose("ManageClient.GetProjects", "LEAVE");

        return result;
    }

    /// <summary>
    /// Gets project associated with project Id
    /// </summary>
    /// <param name="projectId">Id of Project</param>
    /// <returns><see cref="ProjectResponse"/></returns>
    public async Task<ProjectResponse> GetProject(string projectId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("ManageClient.GetProject", "ENTER");
        Log.Information("GetProject", $"projectId: {projectId}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}");
        var result = await GetAsync<ProjectResponse>(uri, cancellationToken, addons, headers);

        Log.Information("GetProject", $"{uri} Succeeded");
        Log.Debug("GetProject", $"result: {result}");
        Log.Verbose("ManageClient.GetProject", "LEAVE");

        return result;
    }

    /// <summary>
    /// Update a project associated with the projectID
    /// </summary>
    /// <param name="projectId">ID of project</param>
    /// <param name="updateProjectSchema"><see cref="ProjectSchema"/> for project</param>
    /// <returns><see cref="MessageResponse"/></returns>
    // USES PATCH
    public async Task<MessageResponse> UpdateProject(string projectId, ProjectSchema updateProjectSchema, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("ManageClient.UpdateProject", "ENTER");
        Log.Information("UpdateProject", $"projectId: {projectId}");
        Log.Information("UpdateProject", $"updateProjectSchema:\n{JsonSerializer.Serialize(updateProjectSchema, JsonSerializeOptions.DefaultOptions)}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}");
        var result = await PatchAsync<ProjectSchema, MessageResponse>(uri, updateProjectSchema, cancellationToken, addons, headers);

        Log.Information("UpdateProject", $"{uri} Succeeded");
        Log.Debug("UpdateProject", $"result: {result}");
        Log.Verbose("ManageClient.UpdateProject", "LEAVE");

        return result;
    }

    /// <summary>
    /// Deletes a project, no response will be returned
    /// </summary>
    /// <param name="projectId">Id of project</param>
    // No response expected
    public async Task<MessageResponse> DeleteProject(string projectId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("ManageClient.DeleteProject", "ENTER");
        Log.Information("DeleteProject", $"projectId: {projectId}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}");
        var result = await DeleteAsync<MessageResponse>(uri, cancellationToken, addons, headers);

        Log.Information("DeleteProject", $"{uri} Succeeded");
        Log.Debug("DeleteProject", $"result: {result}");
        Log.Verbose("ManageClient.DeleteProject", "LEAVE");

        return result;
    }

    /// <summary>
    /// leave project associated with the project Id
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="MessageResponse"/></returns>
    public async Task<MessageResponse> LeaveProject(string projectId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("ManageClient.LeaveProject", "ENTER");
        Log.Information("LeaveProject", $"projectId: {projectId}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/leave");
        var result = await DeleteAsync<MessageResponse>(uri, cancellationToken, addons, headers);

        Log.Information("LeaveProject", $"{uri} Succeeded");
        Log.Debug("LeaveProject", $"result: {result}");
        Log.Verbose("ManageClient.LeaveProject", "LEAVE");

        return result;
    }
    #endregion

    #region ProjectKeys

    /// <summary>
    /// Get the keys associated with the project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="KeysResponse"/></returns>
    public async Task<KeysResponse> GetKeys(string projectId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("ManageClient.GetKeys", "ENTER");
        Log.Information("GetKeys", $"projectId: {projectId}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.KEYS}");
        var result = await GetAsync<KeysResponse>(uri, cancellationToken, addons, headers);

        Log.Information("GetKeys", $"{uri} Succeeded");
        Log.Debug("GetKeys", $"result: {result}");
        Log.Verbose("ManageClient.GetKeys", "LEAVE");

        return result;
    }

    /// <summary>
    /// Get details of key associated with the key ID
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="keyId">Id of key</param>
    /// <returns><see cref="KeyScopeResponse"/></returns>
    public async Task<KeyScopeResponse> GetKey(string projectId, string keyId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("ManageClient.GetKey", "ENTER");
        Log.Information("GetKey", $"projectId: {projectId}");
        Log.Information("GetKey", $"keyId: {keyId}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.KEYS}/{keyId}");
        var result = await GetAsync<KeyScopeResponse>(uri, cancellationToken, addons, headers);

        Log.Information("GetKey", $"{uri} Succeeded");
        Log.Debug("GetKey", $"result: {result}");
        Log.Verbose("ManageClient.GetKey", "LEAVE");

        return result;
    }

    /// <summary>
    /// Create a key in the associated project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="createProjectKeySchema"><see cref="KeySchema"/> for the key to be created</param>
    /// <returns><see cref="KeyResponse"/></returns>
    public async Task<KeyResponse> CreateKey(string projectId, KeySchema keySchema, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("ManageClient.CreateKey", "ENTER");
        Log.Information("CreateKey", $"projectId: {projectId}");
        Log.Information("CreateKey", $"keySchema:\n{JsonSerializer.Serialize(keySchema, JsonSerializeOptions.DefaultOptions)}");

        if (keySchema.ExpirationDate is not null && keySchema.TimeToLiveInSeconds is not null)
        {
            var exStr = "Both ExpirationDate and TimeToLiveInSeconds is set. set either one but not both";
            Log.Error("CreateKey", $"Exception: {exStr}");
            throw new ArgumentException(exStr);
        }

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/keys");
        var result = await PostAsync<KeySchema, KeyResponse>(uri, keySchema, cancellationToken, addons, headers);

        Log.Information("CreateKey", $"{uri} Succeeded");
        Log.Debug("CreateKey", $"result: {result}");
        Log.Verbose("ManageClient.CreateKey", "LEAVE");

        return result;
    }

    /// <summary>
    /// Remove key from project, No response returned
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="keyId">Id of key</param>
    // Nothing being returned
    public async Task<MessageResponse> DeleteKey(string projectId, string keyId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("ManageClient.DeleteKey", "ENTER");
        Log.Information("DeleteKey", $"projectId: {projectId}");
        Log.Information("DeleteKey", $"keyId: {keyId}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/keys/{keyId}");
        var result = await DeleteAsync<MessageResponse>(uri , cancellationToken, addons, headers);

        Log.Information("DeleteKey", $"{uri} Succeeded");
        Log.Debug("DeleteKey", $"result: {result}");
        Log.Verbose("ManageClient.DeleteKey", "LEAVE");

        return result;
    }
    #endregion

    #region ProjectInvites
    /// <summary>
    /// Get any invites that are associated with project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="InvitesResponse"/></returns>
    public async Task<InvitesResponse> GetInvites(string projectId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("ManageClient.GetInvites", "ENTER");
        Log.Information("GetInvites", $"projectId: {projectId}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.INVITES}");
        var result = await GetAsync<InvitesResponse>(uri, cancellationToken, addons, headers);

        Log.Information("GetInvites", $"{uri} Succeeded");
        Log.Debug("GetInvites", $"result: {result}");
        Log.Verbose("ManageClient.GetInvites", "LEAVE");

        return result;
    }

    /// <summary>
    /// Delete a project invite that has been sent
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="email">email of the invite to be removed</param>
    //no response expected
    public async Task<MessageResponse> DeleteInvite(string projectId, string email, CancellationTokenSource? cancellationToken = default,
    Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("ManageClient.DeleteInvite", "ENTER");
        Log.Information("DeleteInvite", $"projectId: {projectId}");
        Log.Information("DeleteInvite", $"email: {email}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.INVITES}/{email}");
        var result = await DeleteAsync<MessageResponse>(uri, cancellationToken, addons, headers);

        Log.Information("DeleteInvite", $"{uri} Succeeded");
        Log.Debug("DeleteInvite", $"result: {result}");
        Log.Verbose("ManageClient.DeleteInvite", "LEAVE");

        return result;
    }

    /// <summary>
    /// Send a invite to the associated project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="inviteSchema"><see cref="InviteSchema"/> for a invite to project</param>
    /// <returns><see cref="MessageResponse"/></returns>
    public async Task<MessageResponse> SendInvite(string projectId, InviteSchema inviteSchema, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("ManageClient.SendInvite", "ENTER");
        Log.Information("SendInvite", $"projectId: {projectId}");
        Log.Information("SendInvite", $"inviteSchema:\n{JsonSerializer.Serialize(inviteSchema, JsonSerializeOptions.DefaultOptions)}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.INVITES}");
        var result = await PostAsync<InviteSchema, MessageResponse>(uri, inviteSchema, cancellationToken, addons, headers);

        Log.Information("SendInvite", $"{uri} Succeeded");
        Log.Debug("SendInvite", $"result: {result}");
        Log.Verbose("ManageClient.SendInvite", "LEAVE");

        return result;
    }
    #endregion

    #region Members
    /// <summary>
    /// Get the members associated with the project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="MembersResponse"/></returns>
    public async Task<MembersResponse> GetMembers(string projectId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("ManageClient.GetMembers", "ENTER");
        Log.Information("GetMembers", $"projectId: {projectId}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.MEMBERS}");
        var result = await GetAsync<MembersResponse>(uri, cancellationToken, addons, headers);

        Log.Information("GetMembers", $"{uri} Succeeded");
        Log.Debug("GetMembers", $"result: {result}");
        Log.Verbose("ManageClient.GetMembers", "LEAVE");

        return result;
    }

    /// <summary>
    /// Get the scopes associated with member
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="memberId">Id of member</param>
    /// <returns><see cref="MemberScopesResponse"/></returns>
    public async Task<MemberScopesResponse> GetMemberScopes(string projectId, string memberId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("ManageClient.GetMemberScopes", "ENTER");
        Log.Information("GetMemberScopes", $"projectId: {projectId}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.MEMBERS}/{memberId}/{UriSegments.SCOPES}");
        var result = await GetAsync<MemberScopesResponse>(uri, cancellationToken, addons, headers);

        Log.Information("GetMemberScopes", $"{uri} Succeeded");
        Log.Debug("GetMemberScopes", $"result: {result}");
        Log.Verbose("ManageClient.GetMemberScopes", "LEAVE");

        return result;
    }

    /// <summary>
    /// Update the scopes fot the member
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="memberId">Id of member</param>
    /// <param name="memberScopeSchema">updates scope options for member<see cref="MemberScopeSchema"/></param>
    /// <returns><see cref="MessageResponse"/></returns>  
    public async Task<MessageResponse> UpdateMemberScope(string projectId, string memberId, MemberScopeSchema scopeSchema,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null)
    {
        Log.Verbose("ManageClient.UpdateMemberScope", "ENTER");
        Log.Information("UpdateMemberScope", $"projectId: {projectId}");
        Log.Information("UpdateMemberScope", $"memberId: {memberId}");
        Log.Information("UpdateMemberScope", $"scopeSchema:\n{JsonSerializer.Serialize(scopeSchema, JsonSerializeOptions.DefaultOptions)}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.MEMBERS}/{memberId}/{UriSegments.SCOPES}");
        var result = await PutAsync<MemberScopeSchema, MessageResponse>(uri,scopeSchema, cancellationToken, addons, headers);

        Log.Information("UpdateMemberScope", $"{uri} Succeeded");
        Log.Debug("UpdateMemberScope", $"result: {result}");
        Log.Verbose("ManageClient.UpdateMemberScope", "LEAVE");

        return result;
    }

    /// <summary>
    /// Remove member from project, there is no response
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="memberId">Id of member</param>   
    //No response expected
    public async Task<MessageResponse> RemoveMember(string projectId, string memberId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("ManageClient.RemoveMember", "ENTER");
        Log.Information("RemoveMember", $"projectId: {projectId}");
        Log.Information("RemoveMember", $"memberId: {memberId}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.MEMBERS}/{memberId}");
        var result = await DeleteAsync<MessageResponse>(uri, cancellationToken, addons, headers);

        Log.Information("RemoveMember", $"{uri} Succeeded");
        Log.Debug("RemoveMember", $"result: {result}");
        Log.Verbose("ManageClient.RemoveMember", "LEAVE");

        return result;
    }
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
        Log.Verbose("ManageClient.GetUsageRequests", "ENTER");
        Log.Information("GetUsageRequests", $"projectId: {projectId}");
        Log.Information("GetUsageRequests", $"usageRequestsSchema:\n{JsonSerializer.Serialize(usageRequestsSchema, JsonSerializeOptions.DefaultOptions)}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.REQUESTS}");
        var result = await GetAsync<UsageRequestsSchema, UsageRequestsResponse>(uri, usageRequestsSchema, cancellationToken, addons, headers);

        Log.Information("GetUsageRequests", $"{uri} Succeeded");
        Log.Debug("GetUsageRequests", $"result: {result}");
        Log.Verbose("ManageClient.GetUsageRequests", "LEAVE");

        return result;
    }

    /// <summary>
    /// Get the details associated with the requestID
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="requestId">Id of request</param>
    /// <returns><see cref="UsageRequestResponse"/></returns>
    public async Task<UsageRequestResponse> GetUsageRequest(string projectId, string requestId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("ManageClient.GetUsageRequest", "ENTER");
        Log.Information("GetUsageRequest", $"projectId: {projectId}");
        Log.Information("GetUsageRequest", $"requestId: {requestId}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.REQUESTS}/{requestId}");
        var result = await GetAsync<UsageRequestResponse>(uri, cancellationToken, addons, headers);

        Log.Information("GetUsageRequest", $"{uri} Succeeded");
        Log.Debug("GetUsageRequest", $"result: {result}");
        Log.Verbose("ManageClient.GetUsageRequest", "LEAVE");

        return result;
    }

    /// <summary>
    /// Gets a summary of usage
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="getUsageSummarySchema">Usage summary options<see cref="UsageSummarySchema"/> </param>
    /// <returns><see cref="UsageSummaryResponse"/></returns>
    public async Task<UsageSummaryResponse> GetUsageSummary(string projectId, UsageSummarySchema summarySchema,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("ManageClient.GetUsageSummary", "ENTER");
        Log.Information("GetUsageSummary", $"projectId: {projectId}");
        Log.Information("GetUsageSummary", $"summarySchema:\n{JsonSerializer.Serialize(summarySchema, JsonSerializeOptions.DefaultOptions)}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.USAGE}");
        var result = await GetAsync<UsageSummarySchema, UsageSummaryResponse>(uri, summarySchema, cancellationToken, addons, headers);

        Log.Information("GetUsageSummary", $"{uri} Succeeded");
        Log.Debug("GetUsageSummary", $"result: {result}");
        Log.Verbose("ManageClient.GetUsageSummary", "LEAVE");

        return result;
    }

    /// <summary>
    /// Get usage fields 
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="getUsageFieldsSchema">Project usage request field options<see cref="UsageFieldsSchema"/></param>
    /// <returns><see cref="UsageFieldsResponse"/></returns>
    public async Task<UsageFieldsResponse> GetUsageFields(string projectId, UsageFieldsSchema fieldsSchema,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("ManageClient.GetUsageFields", "ENTER");
        Log.Information("GetUsageFields", $"projectId: {projectId}");
        Log.Information("GetUsageFields", $"summarySchema:\n{JsonSerializer.Serialize(fieldsSchema, JsonSerializeOptions.DefaultOptions)}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.USAGE}/fields");
        var result = await GetAsync<UsageFieldsSchema, UsageFieldsResponse>(uri, fieldsSchema, cancellationToken, addons, headers);

        Log.Information("GetUsageFields", $"{uri} Succeeded");
        Log.Debug("GetUsageFields", $"result: {result}");
        Log.Verbose("ManageClient.GetUsageFields", "LEAVE");

        return result;
    }
    #endregion

    #region Balances
    /// <summary>
    /// Gets a list of balances
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="BalancesResponse"/></returns>
    public async Task<BalancesResponse> GetBalances(string projectId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("ManageClient.GetBalances", "ENTER");
        Log.Information("GetBalances", $"projectId: {projectId}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.BALANCES}");
        var result = await GetAsync<BalancesResponse>(uri, cancellationToken, addons, headers);

        Log.Information("GetBalances", $"{uri} Succeeded");
        Log.Debug("GetBalances", $"result: {result}");
        Log.Verbose("ManageClient.GetBalances", "LEAVE");

        return result;
    }

    /// <summary>
    /// Get the balance details associated with the balance id
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="balanceId">Id of balance</param>
    /// <returns><see cref="BalanceResponse"/></returns>
    public async Task<BalanceResponse> GetBalance(string projectId, string balanceId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("ManageClient.GetBalance", "ENTER");
        Log.Information("GetBalance", $"projectId: {projectId}");
        Log.Information("GetBalance", $"balanceId: {balanceId}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.BALANCES}/{balanceId}");
        var result = await GetAsync<BalanceResponse>(uri, cancellationToken, addons, headers);

        Log.Information("GetBalances", $"{uri} Succeeded");
        Log.Debug("GetBalance", $"result: {result}");
        Log.Verbose("ManageClient.GetBalance", "LEAVE");

        return result;
    }
    #endregion
}
