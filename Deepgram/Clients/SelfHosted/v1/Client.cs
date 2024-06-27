// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.SelfHosted.v1;
using Deepgram.Clients.Interfaces.v1;

namespace Deepgram.Clients.SelfHosted.v1;

/// <summary>
/// Implements version 1 of the OnPrem Client.
/// </summary>
/// <param name="apiKey">Required DeepgramApiKey</param>
/// <param name="deepgramClientOptions"><see cref="DeepgramHttpClientOptions"/> for HttpClient Configuration</param>
public class Client(string? apiKey = null, IDeepgramClientOptions? deepgramClientOptions = null, string? httpId = null)
    : AbstractRestClient(apiKey, deepgramClientOptions, httpId), ISelfHostedClient
{
    /// <summary>
    /// get a list of credentials associated with project
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <returns><see cref="CredentialsResponse"/></returns>
    public async Task<CredentialsResponse> ListCredentials(string projectId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("OnPremClient.ListCredentials", "ENTER");
        Log.Debug("ListCredentials", $"projectId: {projectId}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.SELF_HOSTED}");
        var result = await GetAsync<CredentialsResponse>(uri, cancellationToken, addons, headers);

        Log.Information("ListCredentials", $"{uri} Succeeded");
        Log.Debug("ListCredentials", $"result: {result}");
        Log.Verbose("OnPremClient.ListCredentials", "LEAVE");

        return result;
    }

    /// <summary>
    /// Get credentials for the project that is associated with credential ID
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="credentialsId">Id of credentials</param>
    /// <returns><see cref="CredentialResponse"/></returns>
    public async Task<CredentialResponse> GetCredentials(string projectId, string credentialsId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("OnPremClient.GetCredentials", "ENTER");
        Log.Debug("GetCredentials", $"projectId: {projectId}");
        Log.Debug("GetCredentials", $"credentialsId: {credentialsId}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.SELF_HOSTED}/{credentialsId}");
        var result = await GetAsync<CredentialResponse>(uri, cancellationToken, addons, headers);

        Log.Information("GetCredentials", $"{uri} Succeeded");
        Log.Debug("GetCredentials", $"result: {result}");
        Log.Verbose("OnPremClient.GetCredentials", "LEAVE");

        return result;
    }

    /// <summary>
    /// Remove credentials in the project associated with the credentials ID
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="credentialsId">Id of credentials</param>
    /// <returns><see cref="MessageResponse"/></returns>
    public async Task<MessageResponse> DeleteCredentials(string projectId, string credentialsId, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("OnPremClient.DeleteCredentials", "ENTER");
        Log.Debug("DeleteCredentials", $"projectId: {projectId}");
        Log.Debug("DeleteCredentials", $"credentialsId: {credentialsId}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.SELF_HOSTED}/{credentialsId}");
        var result = await DeleteAsync<MessageResponse>(uri, cancellationToken, addons, headers);

        Log.Information("DeleteCredentials", $"{uri} Succeeded");
        Log.Debug("DeleteCredentials", $"result: {result}");
        Log.Verbose("OnPremClient.DeleteCredentials", "LEAVE");

        return result;
    }

    /// <summary>
    /// Create credentials for the associated projects
    /// </summary>
    /// <param name="projectId">Id of project</param>
    /// <param name="createOnPremCredentialsSchema"><see cref="CredentialsSchema"/> for credentials to be created</param>
    /// <returns><see cref="CredentialResponse"/></returns>
    public async Task<CredentialResponse> CreateCredentials(string projectId, CredentialsSchema credentialsSchema, 
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("OnPremClient.CreateCredentials", "ENTER");
        Log.Debug("CreateCredentials", $"projectId: {projectId}");
        Log.Debug("CreateCredentials", $"credentialsSchema:\n{credentialsSchema}");

        var uri = GetUri(_options, $"{UriSegments.PROJECTS}/{projectId}/{UriSegments.SELF_HOSTED}");
        var result = await PostAsync<CredentialsSchema, CredentialResponse>(uri, credentialsSchema, cancellationToken, addons, headers);

        Log.Information("CreateCredentials", $"{uri} Succeeded");
        Log.Debug("CreateCredentials", $"result: {result}");
        Log.Verbose("OnPremClient.CreateCredentials", "LEAVE");

        return result;
    }
}
