namespace Deepgram.Clients;


// working of node sdk - https://github.com/deepgram/deepgram-node-sdk/blob/lo/beta-test-improvements/src/packages/ManageClient.ts
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
    public async Task<GetProjectsResponse> GetProjects()
    {
        string url = $"{Constants.PROJECTS}";
        throw new NotImplementedException();
    }

    public async Task<GetProjectResponse> GetProject(string id)
    {
        string url = $"{Constants.PROJECTS}/{id}";
        throw new NotImplementedException();
    }

    public async Task<MessageResponse> UpdateProject(string id, UpdateProjectSchema updateProjectSchema)
    {

        string url = $"{Constants.PROJECTS}/{id}";

        // USES PATCH

        throw new NotImplementedException();
    }

    public async Task DeleteProject(string id)
    {
        string url = $"{Constants.PROJECTS}/{id}";

        // No response expected
        throw new NotImplementedException();
    }

    public async Task<MessageResponse> LeaveProject(string projectId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/leave";
        throw new NotImplementedException();
    }

    #endregion

    #region ProjectKeys
    public async Task<GetProjectKeysResponse> GetProjectKeys(string projectId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.KEYS}";
        throw new NotImplementedException();
    }

    public async Task<GetProjectKeyResponse> GetProjectKey(string projectId, string keyId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.KEYS}/{keyId}";
        throw new NotImplementedException();
    }

    public async Task<CreateProjectKeyResponse> CreateProjectKey(string projectId, CreateProjectKeySchema createProjectKeySchema)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/keys";
        throw new NotImplementedException();
    }
    public async Task DeleteProjectKey(string projectId, string keyId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/keys/{keyId}";
        throw new NotImplementedException();
    }
    #endregion

    #region ProjectInvites
    public async Task<GetProjectInvitesResponse> GetProjectInvites(string projectId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.INVITES}";
        throw new NotImplementedException();
    }

    public async Task DeleteProjectInvite(string projectId, string email)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/invites/{email}";
        throw new NotImplementedException();
    }


    public async Task<MessageResponse> SendProjectInvite(string projectId, SendProjectInviteSchema sendProjectInviteSchema)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/invites";
        throw new NotImplementedException();
    }


    #endregion

    #region Members
    public async Task<GetProjectMembersResponse> GetProjectMembers(string projectId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.MEMBERS}";
        throw new NotImplementedException();
    }

    public async Task<GetProjectMemberScopesResponse> GetProjectMemberScopes(string projectId, string memberId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.MEMBERS}/{memberId}/{Constants.SCOPES}";
        throw new NotImplementedException();
    }

    public async Task<MessageResponse> UpdateProjectMemberScope(string projectId, string memberId, UpdateProjectMemberScopeSchema updateProjectMemberScopeSchema)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.MEMBERS}/{memberId}/{Constants.SCOPES}";

        //uses PUT
        throw new NotImplementedException();
    }

    public async Task RemoveProjectMember(string projectId, string memberId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.MEMBERS}/{memberId}";
        throw new NotImplementedException();
    }
    #endregion

    #region Usage
    public async Task<GetProjectUsageRequestsResponse> GetProjectUsageRequests(string projectId, GetProjectUsageRequestsSchema getProjectUsageRequestsSchema)
    {
        var stringedOptions = QueryParameterUtil.GetParameters(getProjectUsageRequestsSchema);
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.REQUESTS}?{stringedOptions}";
        throw new NotImplementedException();
    }
    public async Task<GetProjectUsageRequestResponse> GetProjectUsageRequest(string projectId, string requestId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.REQUESTS}/{requestId}";
        throw new NotImplementedException();
    }
    public async Task<GetProjectUsageSummaryResponse> GetProjectUsageSummary(string projectId, GetProjectUsageSummarySchema getProjectUsageSummarySchema)
    {
        var stringedOptions = QueryParameterUtil.GetParameters(getProjectUsageSummarySchema);
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.USAGE}?{stringedOptions}";
        throw new NotImplementedException();
    }
    public async Task<GetProjectUsageFieldsResponse> GetProjectUsageFields(string projectId, GetProjectUsageFieldsSchema getProjectUsageFieldsSchema)
    {
        var stringedOptions = QueryParameterUtil.GetParameters(getProjectUsageFieldsSchema);
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.USAGE}/fields?{stringedOptions}";

        throw new NotImplementedException();
    }
    #endregion

    #region Balances
    public async Task<GetProjectBalancesResponse> GetProjectBalances(string projectId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.BALANCES}";
        throw new NotImplementedException();
    }
    public async Task<GetProjectBalanceResponse> GetProjectBalance(string projectId, string balanceId)
    {
        string url = $"{Constants.PROJECTS}/{projectId}/{Constants.BALANCES}/{balanceId}";
        throw new NotImplementedException();
    }
    #endregion

}
