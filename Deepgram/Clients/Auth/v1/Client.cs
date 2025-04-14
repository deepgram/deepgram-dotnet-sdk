// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Auth.v1;
using Deepgram.Clients.Interfaces.v1;
using Deepgram.Abstractions.v1;

namespace Deepgram.Clients.Auth.v1;

/// <summary>
/// Implements version 1 of the Models Client.
/// </summary>
/// <param name="apiKey">Required DeepgramApiKey</param>
/// <param name="deepgramClientOptions"><see cref="DeepgramHttpClientOptions"/> for HttpClient Configuration</param>
public class Client(string? apiKey = null, IDeepgramClientOptions? deepgramClientOptions = null, string? httpId = null)
    : AbstractRestClient(apiKey, deepgramClientOptions, httpId), IAuthClient
{
    /// <summary>
    /// Gets a temporary JWT for the Deepgram API.
    /// </summary>
    /// <returns><see cref="GrantTokenResponse"/></returns>
    public async Task<GrantTokenResponse> GrantToken(CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("AuthClient.GrantToken", "ENTER");

        var uri = GetUri(_options, $"auth/{UriSegments.GRANTTOKEN}");
        var result = await PostAsync<object, GrantTokenResponse>(uri, null, cancellationToken, addons, headers);

        Log.Information("GrantToken", $"{uri} Succeeded");
        Log.Debug("GrantToken", $"result: {result}");
        Log.Verbose("AuthClient.GrantToken", "LEAVE");

        return result;
    }
}
