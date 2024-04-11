// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;
using Deepgram.Clients.Interfaces.v1;

namespace Deepgram;

public static class ClientFactory
{
    /// <summary>
    /// Create a new AnalyzeClient
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="options"></param>
    /// <param name="httpId"></param>
    /// <returns></returns>
    public static IAnalyzeClient CreateAnalyzeClient(string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null)
    {
        return new AnalyzeClient(apiKey, options, httpId);
    }

    /// <summary>
    /// Create a new AnalyzeClient
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ILiveClient CreateLiveClient(string apiKey = "", DeepgramWsClientOptions? options = null)
    {
        return new LiveClient(apiKey, options);
    }

    /// <summary>
    /// Create a new LiveClient
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="options"></param>
    /// <param name="httpId"></param>
    /// <returns></returns>
    public static IManageClient CreateManageClient(string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null)
    {
        return new ManageClient(apiKey, options, httpId);
    }

    /// <summary>
    /// Create a new OnPremClient
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="options"></param>
    /// <param name="httpId"></param>
    /// <returns></returns>
    public static IOnPremClient CreateOnPremClient(string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null)
    {
        return new OnPremClient(apiKey, options, httpId);
    }

    /// <summary>
    /// Create a new PreRecordedClient
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="options"></param>
    /// <param name="httpId"></param>
    /// <returns></returns>
    public static IPreRecordedClient CreatePreRecordedClient(string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null)
    {
        return new PreRecordedClient(apiKey, options, httpId);
    }

    /// <summary>
    /// Create a new SpeakClient
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="options"></param>
    /// <param name="httpId"></param>
    /// <returns></returns>
    public static ISpeakClient CreateSpeakClient(string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null)
    {
        return new SpeakClient(apiKey, options, httpId);
    }
}
