// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.SelfHosted.v1;

namespace Deepgram.Clients.Interfaces.v1;

/// <summary>
/// Manages current self-hosted distribution credentials at
/// <c>/v1/projects/{project_id}/self-hosted/distribution/credentials</c>.
/// </summary>
public interface ISelfHostedDistributionCredentialsClient
{
    /// <summary>
    /// Lists distribution credentials for a project.
    /// </summary>
    public Task<CredentialsResponse> ListDistributionCredentials(string projectId,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null);

    /// <summary>
    /// Gets one distribution credential set.
    /// </summary>
    public Task<CredentialResponse> GetDistributionCredentials(string projectId, string distributionCredentialsId,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null);

    /// <summary>
    /// Creates distribution credentials using query options for scopes and provider.
    /// </summary>
    public Task<CredentialResponse> CreateDistributionCredentials(string projectId,
        DistributionCredentialsCreateSchema? credentialsSchema = null,
        DistributionCredentialsCreateOptions? options = null, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Deletes one distribution credential set.
    /// </summary>
    public Task<CredentialResponse> DeleteDistributionCredentials(string projectId, string distributionCredentialsId,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null);
}
