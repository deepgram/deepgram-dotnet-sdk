// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.AgentManage.v1;

namespace Deepgram.Clients.Interfaces.v1;

/// <summary>
/// Implements version 1 of the Agent Manage Client: REST management of reusable Voice Agent
/// configurations (/v1/projects/{project_id}/agents) and their template variables
/// (/v1/projects/{project_id}/agent-variables).
/// <see href="https://developers.deepgram.com/docs/reusable-agent-configurations"/>
/// </summary>
public interface IAgentManageClient
{
    #region Agent Configurations
    /// <summary>
    /// Gets all reusable agent configurations for the project, in their uninterpolated form.
    /// </summary>
    /// <param name="projectId">Id of Project</param>
    /// <returns><see cref="AgentConfigurationsResponse"/></returns>
    public Task<AgentConfigurationsResponse> GetAgents(string projectId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Gets the specified agent configuration in its uninterpolated form.
    /// </summary>
    /// <param name="projectId">Id of Project</param>
    /// <param name="agentId">Id of the agent configuration</param>
    /// <returns><see cref="AgentConfigurationResponse"/></returns>
    public Task<AgentConfigurationResponse> GetAgent(string projectId, string agentId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Creates a new reusable agent configuration.
    /// </summary>
    /// <param name="projectId">Id of Project</param>
    /// <param name="configurationSchema"><see cref="AgentConfigurationSchema"/> describing the configuration to create</param>
    /// <returns><see cref="AgentConfigurationResponse"/></returns>
    public Task<AgentConfigurationResponse> CreateAgent(string projectId, AgentConfigurationSchema configurationSchema,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Updates the metadata associated with an agent configuration. The config itself is
    /// immutable.
    /// </summary>
    /// <param name="projectId">Id of Project</param>
    /// <param name="agentId">Id of the agent configuration</param>
    /// <param name="metadataSchema"><see cref="AgentMetadataSchema"/> with the replacement metadata</param>
    /// <returns><see cref="AgentConfigurationResponse"/></returns>
    public Task<AgentConfigurationResponse> UpdateAgentMetadata(string projectId, string agentId, AgentMetadataSchema metadataSchema,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Deletes the specified agent configuration.
    /// </summary>
    /// <param name="projectId">Id of Project</param>
    /// <param name="agentId">Id of the agent configuration</param>
    /// <returns><see cref="DeleteResponse"/></returns>
    public Task<DeleteResponse> DeleteAgent(string projectId, string agentId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);
    #endregion

    #region Agent Variables
    /// <summary>
    /// Gets all template variables for the project.
    /// </summary>
    /// <param name="projectId">Id of Project</param>
    /// <returns><see cref="AgentVariablesResponse"/></returns>
    public Task<AgentVariablesResponse> GetAgentVariables(string projectId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Gets the specified template variable.
    /// </summary>
    /// <param name="projectId">Id of Project</param>
    /// <param name="variableId">Id of the template variable</param>
    /// <returns><see cref="AgentVariableResponse"/></returns>
    public Task<AgentVariableResponse> GetAgentVariable(string projectId, string variableId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Creates a new template variable.
    /// </summary>
    /// <param name="projectId">Id of Project</param>
    /// <param name="variableSchema"><see cref="AgentVariableSchema"/> describing the variable to create</param>
    /// <returns><see cref="AgentVariableResponse"/></returns>
    public Task<AgentVariableResponse> CreateAgentVariable(string projectId, AgentVariableSchema variableSchema,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Updates the value of an existing template variable.
    /// </summary>
    /// <param name="projectId">Id of Project</param>
    /// <param name="variableId">Id of the template variable</param>
    /// <param name="updateSchema"><see cref="UpdateAgentVariableSchema"/> with the new value</param>
    /// <returns><see cref="AgentVariableResponse"/></returns>
    public Task<AgentVariableResponse> UpdateAgentVariable(string projectId, string variableId, UpdateAgentVariableSchema updateSchema,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Deletes the specified template variable.
    /// </summary>
    /// <param name="projectId">Id of Project</param>
    /// <param name="variableId">Id of the template variable</param>
    /// <returns><see cref="DeleteResponse"/></returns>
    public Task<DeleteResponse> DeleteAgentVariable(string projectId, string variableId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);
    #endregion
}
