// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Abstractions.v1;
using Deepgram.Clients.Interfaces.v1;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.SelfHosted.v1;

namespace Deepgram.Clients.SelfHosted.v1;

/// <summary>
/// Implements the current self-hosted distribution credentials API. This client is separate from
/// <see cref="Client"/> so existing callers retain the legacy on-prem route and request shape.
/// </summary>
public class DistributionCredentialsClient(string? apiKey = null, IDeepgramClientOptions? deepgramClientOptions = null,
    string? httpId = null) : AbstractRestClient(apiKey, deepgramClientOptions, httpId), ISelfHostedDistributionCredentialsClient
{
    /// <summary>
    /// Credential-management responses can include sensitive distribution data, so do not write
    /// their bodies to diagnostic logs.
    /// </summary>
    protected override bool LogResponseBodies => false;

    /// <inheritdoc/>
    public async Task<CredentialsResponse> ListDistributionCredentials(string projectId,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null)
    {
        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.DISTRIBUTION_CREDENTIALS}");
        return await GetAsync<CredentialsResponse>(uri, cancellationToken, addons, headers);
    }

    /// <inheritdoc/>
    public async Task<CredentialResponse> GetDistributionCredentials(string projectId, string distributionCredentialsId,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null)
    {
        var uri = GetUri(_options,
            $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.DISTRIBUTION_CREDENTIALS}/{distributionCredentialsId}");
        return await GetAsync<CredentialResponse>(uri, cancellationToken, addons, headers);
    }

    /// <inheritdoc/>
    public async Task<CredentialResponse> CreateDistributionCredentials(string projectId,
        DistributionCredentialsCreateSchema? credentialsSchema = null,
        DistributionCredentialsCreateOptions? options = null, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.DISTRIBUTION_CREDENTIALS}");
        return await PostAsync<DistributionCredentialsCreateSchema, DistributionCredentialsCreateOptions, CredentialResponse>(
            uri, options, credentialsSchema ?? new DistributionCredentialsCreateSchema(), cancellationToken, addons, headers);
    }

    /// <inheritdoc/>
    public async Task<CredentialResponse> DeleteDistributionCredentials(string projectId, string distributionCredentialsId,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null)
    {
        var uri = GetUri(_options,
            $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.DISTRIBUTION_CREDENTIALS}/{distributionCredentialsId}");
        return await DeleteAsync<CredentialResponse>(uri, cancellationToken, addons, headers);
    }
}
