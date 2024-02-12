// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Manage.v1;
using Deepgram.Models.OnPrem.v1;

namespace Deepgram;

/// <summary>
/// Constructor for the client that communicates with the on premise Deepgram Server
/// </summary>
/// <param name="apiKey">Required DeepgramApiKey</param>
/// <param name="deepgramClientOptions"><see cref="DeepgramClientOptions"/> for HttpClient Configuration</param>
public class OnPremClient(string apiKey, DeepgramClientOptions? deepgramClientOptions = null)
    : AbstractRestClient(apiKey, deepgramClientOptions)
{


    /// <summary>
    /// get a list of credentials associated with project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="CredentialsResponse"/></returns>
    public async Task<CredentialsResponse> ListCredentials(string projectId, CancellationToken cancellationToken = default) =>
        await GetAsync<CredentialsResponse>($"{UriSegments.PROJECTS}/{projectId}/{UriSegments.ONPREM}", cancellationToken);

    /// <summary>
    /// Get credentials for the project that is associated with credential ID
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="credentialsId">Id of credentials</param>
    /// <returns><see cref="CredentialResponse"/></returns>
    public async Task<CredentialResponse> GetCredentials(string projectId, string credentialsId, CancellationToken cancellationToken = default) =>
        await GetAsync<CredentialResponse>($"{UriSegments.PROJECTS}/{projectId}/{UriSegments.ONPREM}/{credentialsId}", cancellationToken);

    /// <summary>
    /// Remove credentials in the project associated with the credentials ID
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="credentialsId">Id of credentials</param>
    /// <returns><see cref="MessageResponse"/></returns>
    public async Task<MessageResponse> DeleteCredentials(string projectId, string credentialsId, CancellationToken cancellationToken = default) =>
        await DeleteAsync<MessageResponse>($"{UriSegments.PROJECTS}/{projectId}/{UriSegments.ONPREM}/{credentialsId}", cancellationToken);

    /// <summary>
    /// Create credentials for the associated projects
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="createOnPremCredentialsSchema"><see cref="CredentialsSchema"/> for credentials to be created</param>
    /// <returns><see cref="CredentialResponse"/></returns>
    public async Task<CredentialResponse> CreateCredentials(string projectId, CredentialsSchema createOnPremCredentialsSchema, CancellationToken cancellationToken = default) =>
        await PostAsync<CredentialResponse>(
            $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.ONPREM}",
            RequestContentUtil.CreatePayload(createOnPremCredentialsSchema), cancellationToken);

}
