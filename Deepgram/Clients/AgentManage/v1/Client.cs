// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.AgentManage.v1;
using Deepgram.Models.Exceptions.v1;
using Deepgram.Clients.Interfaces.v1;
using Deepgram.Abstractions.v1;

namespace Deepgram.Clients.AgentManage.v1;

/// <summary>
/// Implements version 1 of the Agent Manage Client: REST management of reusable Voice Agent
/// configurations (/v1/projects/{project_id}/agents) and their template variables
/// (/v1/projects/{project_id}/agent-variables).
///
/// Logging note: this client never logs request or response payloads. Agent configurations
/// carry prompts, metadata, and function-endpoint headers, and variable values can carry
/// arbitrary customer data — none of that belongs in default SDK logs. Only operation names
/// and resource IDs are logged.
/// <see href="https://developers.deepgram.com/docs/reusable-agent-configurations"/>
/// </summary>
/// <param name="apiKey">Required DeepgramApiKey</param>
/// <param name="deepgramClientOptions"><see cref="DeepgramHttpClientOptions"/> for HttpClient Configuration</param>
public class Client(string? apiKey = null, IDeepgramClientOptions? deepgramClientOptions = null, string? httpId = null)
    : AbstractRestClient(apiKey, deepgramClientOptions, httpId), IAgentManageClient
{
    #region Agent Configurations
    /// <summary>
    /// Gets all reusable agent configurations for the project. Configurations are returned in
    /// their uninterpolated form — template variable references appear as-is rather than
    /// with their substituted values.
    /// </summary>
    /// <param name="projectId">Id of Project</param>
    /// <returns><see cref="AgentConfigurationsResponse"/></returns>
    public async Task<AgentConfigurationsResponse> GetAgents(string projectId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("AgentManageClient.GetAgents", "ENTER");
        Log.Information("GetAgents", $"projectId: {projectId}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.AGENTS}");
        var result = await GetAsync<AgentConfigurationsResponse>(uri, cancellationToken, addons, headers);

        Log.Information("GetAgents", $"{uri} Succeeded");
        Log.Debug("GetAgents", $"returned {result?.Agents?.Count ?? 0} agent configuration(s)");
        Log.Verbose("AgentManageClient.GetAgents", "LEAVE");

        return result!;
    }

    /// <summary>
    /// Gets the specified agent configuration in its uninterpolated form.
    /// </summary>
    /// <param name="projectId">Id of Project</param>
    /// <param name="agentId">Id of the agent configuration</param>
    /// <returns><see cref="AgentConfigurationResponse"/></returns>
    public async Task<AgentConfigurationResponse> GetAgent(string projectId, string agentId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("AgentManageClient.GetAgent", "ENTER");
        Log.Information("GetAgent", $"projectId: {projectId}");
        Log.Information("GetAgent", $"agentId: {agentId}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.AGENTS}/{agentId}");
        var result = await GetAsync<AgentConfigurationResponse>(uri, cancellationToken, addons, headers);

        Log.Information("GetAgent", $"{uri} Succeeded");
        Log.Verbose("AgentManageClient.GetAgent", "LEAVE");

        return result;
    }

    /// <summary>
    /// Creates a new reusable agent configuration. <see cref="AgentConfigurationSchema.Config"/>
    /// must be a valid JSON string representing the agent block of a Voice Agent Settings
    /// message. The returned AgentId can be passed in place of the full agent object in future
    /// Settings messages.
    /// </summary>
    /// <param name="projectId">Id of Project</param>
    /// <param name="configurationSchema"><see cref="AgentConfigurationSchema"/> describing the configuration to create</param>
    /// <returns><see cref="AgentConfigurationResponse"/></returns>
    public async Task<AgentConfigurationResponse> CreateAgent(string projectId, AgentConfigurationSchema configurationSchema,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("AgentManageClient.CreateAgent", "ENTER");
        Log.Information("CreateAgent", $"projectId: {projectId}");

        if (configurationSchema is null)
        {
            throw new ArgumentNullException(nameof(configurationSchema));
        }
        if (string.IsNullOrWhiteSpace(configurationSchema.Config))
        {
            throw new DeepgramException("CreateAgent requires Config to be set to a JSON string representing the agent block of a Settings message.");
        }

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.AGENTS}");
        // The body-only overload keeps the (potentially large) config JSON string out of the
        // query string.
        var result = await PostAsync<AgentConfigurationSchema, NoopSchema, AgentConfigurationResponse>(
            uri, null, configurationSchema, cancellationToken, addons, headers);

        Log.Information("CreateAgent", $"{uri} Succeeded");
        Log.Debug("CreateAgent", $"created agentId: {result?.AgentId}");
        Log.Verbose("AgentManageClient.CreateAgent", "LEAVE");

        return result!;
    }

    /// <summary>
    /// Updates the metadata associated with an agent configuration. The config itself is
    /// immutable — to change the configuration, delete the existing agent and create a new one.
    /// The live API currently returns an empty body for this call, in which case the returned
    /// object has null fields; call GetAgent afterwards if you need the updated state.
    /// </summary>
    /// <param name="projectId">Id of Project</param>
    /// <param name="agentId">Id of the agent configuration</param>
    /// <param name="metadataSchema"><see cref="AgentMetadataSchema"/> with the replacement metadata</param>
    /// <returns><see cref="AgentConfigurationResponse"/></returns>
    // USES PUT
    public async Task<AgentConfigurationResponse> UpdateAgentMetadata(string projectId, string agentId, AgentMetadataSchema metadataSchema,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("AgentManageClient.UpdateAgentMetadata", "ENTER");
        Log.Information("UpdateAgentMetadata", $"projectId: {projectId}");
        Log.Information("UpdateAgentMetadata", $"agentId: {agentId}");

        if (metadataSchema is null)
        {
            throw new ArgumentNullException(nameof(metadataSchema));
        }
        if (metadataSchema.Metadata is null)
        {
            throw new DeepgramException("UpdateAgentMetadata requires Metadata to be set.");
        }

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.AGENTS}/{agentId}");
        // allowEmptyResponseBody: the live API answers this PUT with 200 and an empty body.
        var result = await PutAsync<AgentMetadataSchema, AgentConfigurationResponse>(
            uri, metadataSchema, allowEmptyResponseBody: true, cancellationToken, addons, headers)
            ?? new AgentConfigurationResponse();

        Log.Information("UpdateAgentMetadata", $"{uri} Succeeded");
        Log.Verbose("AgentManageClient.UpdateAgentMetadata", "LEAVE");

        return result;
    }

    /// <summary>
    /// Deletes the specified agent configuration. Deleting an agent configuration can cause a
    /// production outage if your service references this agent UUID — migrate all active
    /// sessions to a new configuration before deleting.
    /// </summary>
    /// <param name="projectId">Id of Project</param>
    /// <param name="agentId">Id of the agent configuration</param>
    /// <returns><see cref="DeleteResponse"/></returns>
    public async Task<DeleteResponse> DeleteAgent(string projectId, string agentId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("AgentManageClient.DeleteAgent", "ENTER");
        Log.Information("DeleteAgent", $"projectId: {projectId}");
        Log.Information("DeleteAgent", $"agentId: {agentId}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.AGENTS}/{agentId}");
        // allowEmptyResponseBody: the live API answers this DELETE with 200 and an empty body.
        var result = await DeleteAsync<DeleteResponse>(uri, allowEmptyResponseBody: true, cancellationToken, addons, headers)
            ?? new DeleteResponse();

        Log.Information("DeleteAgent", $"{uri} Succeeded");
        Log.Verbose("AgentManageClient.DeleteAgent", "LEAVE");

        return result;
    }
    #endregion

    #region Agent Variables
    /// <summary>
    /// Gets all template variables for the project.
    /// </summary>
    /// <param name="projectId">Id of Project</param>
    /// <returns><see cref="AgentVariablesResponse"/></returns>
    public async Task<AgentVariablesResponse> GetAgentVariables(string projectId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("AgentManageClient.GetAgentVariables", "ENTER");
        Log.Information("GetAgentVariables", $"projectId: {projectId}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.AGENT_VARIABLES}");
        var result = await GetAsync<AgentVariablesResponse>(uri, cancellationToken, addons, headers);

        Log.Information("GetAgentVariables", $"{uri} Succeeded");
        Log.Debug("GetAgentVariables", $"returned {result?.Variables?.Count ?? 0} agent variable(s)");
        Log.Verbose("AgentManageClient.GetAgentVariables", "LEAVE");

        return result!;
    }

    /// <summary>
    /// Gets the specified template variable.
    /// </summary>
    /// <param name="projectId">Id of Project</param>
    /// <param name="variableId">Id of the template variable</param>
    /// <returns><see cref="AgentVariableResponse"/></returns>
    public async Task<AgentVariableResponse> GetAgentVariable(string projectId, string variableId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("AgentManageClient.GetAgentVariable", "ENTER");
        Log.Information("GetAgentVariable", $"projectId: {projectId}");
        Log.Information("GetAgentVariable", $"variableId: {variableId}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.AGENT_VARIABLES}/{variableId}");
        var result = await GetAsync<AgentVariableResponse>(uri, cancellationToken, addons, headers);

        Log.Information("GetAgentVariable", $"{uri} Succeeded");
        Log.Verbose("AgentManageClient.GetAgentVariable", "LEAVE");

        return result;
    }

    /// <summary>
    /// Creates a new template variable. Variables follow the DG_&lt;VARIABLE_NAME&gt; naming
    /// format and can substitute any JSON value in an agent configuration.
    /// </summary>
    /// <param name="projectId">Id of Project</param>
    /// <param name="variableSchema"><see cref="AgentVariableSchema"/> describing the variable to create</param>
    /// <returns><see cref="AgentVariableResponse"/></returns>
    public async Task<AgentVariableResponse> CreateAgentVariable(string projectId, AgentVariableSchema variableSchema,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("AgentManageClient.CreateAgentVariable", "ENTER");
        Log.Information("CreateAgentVariable", $"projectId: {projectId}");

        if (variableSchema is null)
        {
            throw new ArgumentNullException(nameof(variableSchema));
        }
        if (string.IsNullOrWhiteSpace(variableSchema.Key))
        {
            throw new DeepgramException("CreateAgentVariable requires Key to be set (DG_<VARIABLE_NAME> format).");
        }
        if (variableSchema.Value is null)
        {
            throw new DeepgramException("CreateAgentVariable requires Value to be set.");
        }

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.AGENT_VARIABLES}");
        // The body-only overload keeps the arbitrary JSON value out of the query string.
        var result = await PostAsync<AgentVariableSchema, NoopSchema, AgentVariableResponse>(
            uri, null, variableSchema, cancellationToken, addons, headers);

        Log.Information("CreateAgentVariable", $"{uri} Succeeded");
        Log.Debug("CreateAgentVariable", $"created variableId: {result?.VariableId}");
        Log.Verbose("AgentManageClient.CreateAgentVariable", "LEAVE");

        return result!;
    }

    /// <summary>
    /// Updates the value of an existing template variable. The live API currently returns an
    /// empty body for this call, in which case the returned object has null fields; call
    /// GetAgentVariable afterwards if you need the updated state.
    /// </summary>
    /// <param name="projectId">Id of Project</param>
    /// <param name="variableId">Id of the template variable</param>
    /// <param name="updateSchema"><see cref="UpdateAgentVariableSchema"/> with the new value</param>
    /// <returns><see cref="AgentVariableResponse"/></returns>
    // USES PATCH
    public async Task<AgentVariableResponse> UpdateAgentVariable(string projectId, string variableId, UpdateAgentVariableSchema updateSchema,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("AgentManageClient.UpdateAgentVariable", "ENTER");
        Log.Information("UpdateAgentVariable", $"projectId: {projectId}");
        Log.Information("UpdateAgentVariable", $"variableId: {variableId}");

        if (updateSchema is null)
        {
            throw new ArgumentNullException(nameof(updateSchema));
        }
        if (updateSchema.Value is null)
        {
            throw new DeepgramException("UpdateAgentVariable requires Value to be set.");
        }

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.AGENT_VARIABLES}/{variableId}");
        // The body-only overload keeps the arbitrary JSON value out of the query string.
        // allowEmptyResponseBody: the live API answers this PATCH with 200 and an empty body.
        var result = await PatchAsync<UpdateAgentVariableSchema, NoopSchema, AgentVariableResponse>(
            uri, null, updateSchema, allowEmptyResponseBody: true, cancellationToken, addons, headers)
            ?? new AgentVariableResponse();

        Log.Information("UpdateAgentVariable", $"{uri} Succeeded");
        Log.Verbose("AgentManageClient.UpdateAgentVariable", "LEAVE");

        return result;
    }

    /// <summary>
    /// Deletes the specified template variable.
    /// </summary>
    /// <param name="projectId">Id of Project</param>
    /// <param name="variableId">Id of the template variable</param>
    /// <returns><see cref="DeleteResponse"/></returns>
    public async Task<DeleteResponse> DeleteAgentVariable(string projectId, string variableId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("AgentManageClient.DeleteAgentVariable", "ENTER");
        Log.Information("DeleteAgentVariable", $"projectId: {projectId}");
        Log.Information("DeleteAgentVariable", $"variableId: {variableId}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.AGENT_VARIABLES}/{variableId}");
        // allowEmptyResponseBody: the live API answers this DELETE with 200 and an empty body.
        var result = await DeleteAsync<DeleteResponse>(uri, allowEmptyResponseBody: true, cancellationToken, addons, headers)
            ?? new DeleteResponse();

        Log.Information("DeleteAgentVariable", $"{uri} Succeeded");
        Log.Verbose("AgentManageClient.DeleteAgentVariable", "LEAVE");

        return result;
    }
    #endregion
}
