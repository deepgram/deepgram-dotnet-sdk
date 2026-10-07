// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Agent.v1.REST;

namespace Deepgram.Clients.Interfaces.v1;

/// <summary>
/// Retrieves current Voice Agent settings catalogs.
/// </summary>
public interface IAgentCatalogClient
{
    /// <summary>
    /// Gets the Think models currently available for Voice Agent sessions.
    /// </summary>
    Task<AgentThinkModelsResponse> GetThinkModels(CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);
}
