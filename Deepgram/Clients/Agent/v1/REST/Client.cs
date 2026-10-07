// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Abstractions.v1;
using Deepgram.Clients.Interfaces.v1;
using Deepgram.Constants;
using Deepgram.Models.Agent.v1.REST;
using Deepgram.Models.Authenticate.v1;

namespace Deepgram.Clients.Agent.v1.REST;

/// <summary>
/// Implements the Voice Agent settings catalog API.
/// </summary>
public class Client(string? apiKey = null, IDeepgramClientOptions? deepgramClientOptions = null, string? httpId = null)
    : AbstractRestClient(apiKey, deepgramClientOptions, httpId), IAgentCatalogClient
{
    /// <summary>
    /// Gets the Think models currently available for Voice Agent sessions.
    /// </summary>
    public async Task<AgentThinkModelsResponse> GetThinkModels(CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("AgentCatalogClient.GetThinkModels", "ENTER");

        var uri = GetUri(_options);
        var result = await GetAsync<AgentThinkModelsResponse>(uri, cancellationToken, addons, headers);

        Log.Information("GetThinkModels", $"{uri} Succeeded");
        Log.Verbose("AgentCatalogClient.GetThinkModels", "LEAVE");
        return result!;
    }

    internal static string GetUri(IDeepgramClientOptions options)
    {
        var uri = new Uri(options.BaseAddress, UriKind.Absolute);
        var builder = new UriBuilder(uri)
        {
            Host = string.Equals(uri.Host, Defaults.DEFAULT_URI, StringComparison.OrdinalIgnoreCase)
                ? UriSegments.AGENT_URI
                : uri.Host,
            Path = UriSegments.THINK_MODELS,
            Query = string.Empty,
        };

        return builder.Uri.AbsoluteUri.TrimEnd('/');
    }
}
