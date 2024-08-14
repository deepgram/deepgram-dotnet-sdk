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
    // *********** WARNING ***********
    // This function creates a LiveClient for the Deepgram API
    //
    // Deprecated: This function is deprecated. Use the `CreateListenWebSocketClient` function instead.
    // This will be removed in a future release.
    //
    // This class is frozen and no new functionality will be added.
    // *********** WARNING ***********
    /// </summary>
    [Obsolete("Please use CreateListenWebSocketClient instead", false)]
    public static ILiveClient CreateLiveClient(string apiKey = "", DeepgramWsClientOptions? options = null)
    {
        return new LiveClient(apiKey, options);
    }

    /// <summary>
    /// Create a new ListenWebSocketClient
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IListenWebSocketClient CreateListenWebSocketClient(string apiKey = "", DeepgramWsClientOptions? options = null)
    {
        return new ListenWebSocketClient(apiKey, options);
    }

    /// <summary>
    /// Create a new ManageClient
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
    // *********** WARNING ***********
    // This function creates a OnPrem Client for the Deepgram API
    //
    // Deprecated: This function is deprecated. Use the `CreateSelfHostedClient` function instead.
    // This will be removed in a future release.
    //
    // This class is frozen and no new functionality will be added.
    // *********** WARNING ***********
    /// </summary>
    [Obsolete("Please use CreateSelfHostedClient instead", false)]
    public static IOnPremClient CreateOnPremClient(string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null)
    {
        return new OnPremClient(apiKey, options, httpId);
    }

    /// <summary>
    /// Create a new SelfHostedClient
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="options"></param>
    /// <param name="httpId"></param>
    /// <returns></returns>
    public static ISelfHostedClient CreateSelfHostedClient(string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null)
    {
        return new SelfHostedClient(apiKey, options, httpId);
    }

    /// <summary>
    // *********** WARNING ***********
    // This function creates a PreRecordedClient for the Deepgram API
    //
    // Deprecated: This function is deprecated. Use the `CreateListenRESTClient` function instead.
    // This will be removed in a future release.
    //
    // This class is frozen and no new functionality will be added.
    // *********** WARNING ***********
    /// </summary>
    [Obsolete("Please use CreateListenRESTClient instead", false)]
    public static IPreRecordedClient CreatePreRecordedClient(string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null)
    {
        return new PreRecordedClient(apiKey, options, httpId);
    }

    /// <summary>
    /// Create a new ListenRESTClient
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="options"></param>
    /// <param name="httpId"></param>
    /// <returns></returns>
    public static IListenRESTClient CreateListenRESTClient(string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null)
    {
        return new ListenRESTClient(apiKey, options, httpId);
    }

    /// <summary>
    // *********** WARNING ***********
    // This function creates a Speak Client for the Deepgram API
    //
    // Deprecated: This function is deprecated. Use the `CreateSpeakRESTClient` function instead.
    // This will be removed in a future release.
    //
    // This class is frozen and no new functionality will be added.
    // *********** WARNING ***********
    /// </summary>
    [Obsolete("Please use CreateSpeakRESTClient instead", false)]
    public static ISpeakClient CreateSpeakClient(string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null)
    {
        return new SpeakClient(apiKey, options, httpId);
    }

    /// <summary>
    /// Create a new SpeakRESTClient
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="options"></param>
    /// <param name="httpId"></param>
    /// <returns></returns>
    public static ISpeakRESTClient CreateSpeakRESTClient(string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null)
    {
        return new SpeakRESTClient(apiKey, options, httpId);
    }

    ///// <summary>
    ///// Create a new AnalyzeClient
    ///// </summary>
    ///// <param name="apiKey"></param>
    ///// <param name="options"></param>
    ///// <returns></returns>
    //public static ISpeakWebSocketClient CreateSpeakWebSocketClient(string apiKey = "", DeepgramWsClientOptions? options = null)
    //{
    //    return new SpeakWebSocketClient(apiKey, options);
    //}
}
