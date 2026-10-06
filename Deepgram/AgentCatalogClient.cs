// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;
using Deepgram.Constants;

namespace Deepgram;

/// <summary>
/// Implements the latest supported version of the Voice Agent settings catalog client.
/// </summary>
public class AgentCatalogClient : Clients.Agent.v1.REST.Client
{
    public AgentCatalogClient(string apiKey = "", DeepgramHttpClientOptions? deepgramClientOptions = null,
        string? httpId = null) : base(apiKey, ResolveOptions(apiKey, deepgramClientOptions), httpId)
    {
    }

    private static DeepgramHttpClientOptions? ResolveOptions(string apiKey, DeepgramHttpClientOptions? options)
    {
        if (options is not null ||
            !string.IsNullOrWhiteSpace(apiKey) ||
            !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(Defaults.DEEPGRAM_ACCESS_TOKEN)) ||
            !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(Defaults.DEEPGRAM_API_KEY)))
        {
            return options;
        }

        // The Think-model catalog is public. OnPrem suppresses the shared options constructor's
        // credential requirement; it does not alter the agent.deepgram.com request route.
        return new DeepgramHttpClientOptions(onPrem: true);
    }
}
