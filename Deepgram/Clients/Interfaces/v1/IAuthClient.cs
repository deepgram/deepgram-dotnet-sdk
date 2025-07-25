// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Auth.v1;

namespace Deepgram.Clients.Interfaces.v1;

/// <summary>
/// Implements version 1 of the Auth Client.
/// </summary>
/// <param name="apiKey">Required DeepgramApiKey</param>
/// <param name="deepgramClientOptions"><see cref="DeepgramHttpClientOptions"/> for HttpClient Configuration</param>
public interface IAuthClient
{
    /// <summary>
    /// Gets a short-lived JWT for the Deepgram API.
    /// </summary>
    /// <returns><see cref="GrantTokenResponse"/></returns>
    public Task<GrantTokenResponse> GrantToken(CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Gets a short-lived JWT for the Deepgram API with custom TTL.
    /// </summary>
    /// <param name="grantTokenSchema"><see cref="GrantTokenSchema"/> for grant token configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="addons">Additional parameters to add to the request</param>
    /// <param name="headers">Additional headers to add to the request</param>
    /// <returns><see cref="GrantTokenResponse"/></returns>
    public Task<GrantTokenResponse> GrantToken(GrantTokenSchema grantTokenSchema,
        CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);
}
