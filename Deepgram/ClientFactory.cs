// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;
using V1 = Deepgram.Clients.Interfaces.v1;
using V2 = Deepgram.Clients.Interfaces.v2;

using ListenV1 = Deepgram.Clients.Listen.v1.WebSocket;
using ListenV2 = Deepgram.Clients.Listen.v2.WebSocket;
using SpeakV1 = Deepgram.Clients.Speak.v1.WebSocket;
using SpeakV2 = Deepgram.Clients.Speak.v2.WebSocket;

namespace Deepgram;

public static class ClientFactory
{
    /// <summary>
    /// Create a new AgentWebSocketClient using the latest version
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static V2.IAgentWebSocketClient CreateAgentWebSocketClient(string apiKey = "", DeepgramWsClientOptions? options = null)
    {
        return new AgentWebSocketClient(apiKey, options);
    }

    /// <summary>
    /// Create a new AnalyzeClient
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="options"></param>
    /// <param name="httpId"></param>
    /// <returns></returns>
    public static V1.IAnalyzeClient CreateAnalyzeClient(string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null)
    {
        return new AnalyzeClient(apiKey, options, httpId);
    }

    /// <summary>
    /// Create a new ListenWebSocketClient using the latest version
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static V2.IListenWebSocketClient CreateListenWebSocketClient(string apiKey = "", DeepgramWsClientOptions? options = null)
    {
        return new ListenWebSocketClient(apiKey, options);
    }

    /// <summary>
    /// Create a new AuthClient
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="options"></param>
    /// <param name="httpId"></param>
    /// <returns></returns>
    public static V1.IAuthClient CreateAuthClient(string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null)
    {
        return new AuthClient(apiKey, options, httpId);
    }

    /// <summary>
    /// Create a new ManageClient
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="options"></param>
    /// <param name="httpId"></param>
    /// <returns></returns>
    public static V1.IManageClient CreateManageClient(string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null)
    {
        return new ManageClient(apiKey, options, httpId);
    }

    /// <summary>
    /// Create a new SelfHostedClient
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="options"></param>
    /// <param name="httpId"></param>
    /// <returns></returns>
    public static V1.ISelfHostedClient CreateSelfHostedClient(string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null)
    {
        return new SelfHostedClient(apiKey, options, httpId);
    }

    /// <summary>
    /// Create a new ListenRESTClient
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="options"></param>
    /// <param name="httpId"></param>
    /// <returns></returns>
    public static V1.IListenRESTClient CreateListenRESTClient(string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null)
    {
        return new ListenRESTClient(apiKey, options, httpId);
    }

    /// <summary>
    /// Create a new SpeakRESTClient
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="options"></param>
    /// <param name="httpId"></param>
    /// <returns></returns>
    public static V1.ISpeakRESTClient CreateSpeakRESTClient(string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null)
    {
        return new SpeakRESTClient(apiKey, options, httpId);
    }

    /// <summary>
    /// Create a new AnalyzeClient
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static V2.ISpeakWebSocketClient CreateSpeakWebSocketClient(string apiKey = "", DeepgramWsClientOptions? options = null)
    {
        return new SpeakWebSocketClient(apiKey, options);
    }



    ///
    /// These functions are only used to create specific and typically older verions of Listen, Speak, etc clients.
    /// The functions above always create the latest version of the client.
    /// 
    /// The functions below should only be used to reference an older client with the intnetion of at some point to move away from
    /// that version to the latest version (ie transition to the function above). Older clients will be removed at the next major version
    /// and the latest version will be renamed to v1.
    /// 

    /// <summary>
    /// This method allows you to create an AgentClient with a specific version of the client.
    /// TODO: this should be revisited at a later time. Opportunity to use reflection to get the type of the client
    /// </summary>
    public static object CreateAgentWebSocketClient(int version, string apiKey = "", DeepgramWsClientOptions? options = null)
    {
        return new AgentWebSocketClient(apiKey, options);
    }

    /// <summary>
    /// This method allows you to create an AnalyzeClient with a specific version of the client.
    /// TODO: this should be revisited at a later time. Opportunity to use reflection to get the type of the client
    /// </summary>
    public static object CreateAnalyzeClient(int version, string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null)
    {
        // Currently only a single version of the AnalyzeClient exists
        return new AnalyzeClient(apiKey, options, httpId);
    }

    /// <summary>
    /// This method allows you to create an ListenRESTClient with a specific version of the client.
    /// TODO: this should be revisited at a later time. Opportunity to use reflection to get the type of the client
    /// </summary>
    public static object CreateListenRESTClient(int version, string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null)
    {
        // Currently only a single version of the ListenRESTClient exists
        return new ListenRESTClient(apiKey, options, httpId);
    }

    /// <summary>
    /// This method allows you to create an AnalyzeClient with a specific version of the client.
    /// TODO: this should be revisited at a later time. Opportunity to use reflection to get the type of the client
    /// </summary>
    public static object CreateListenWebSocketClient(int version, string apiKey = "", DeepgramWsClientOptions? options = null)
    {
        // at some point this needs to be changed to use reflection to get the type of the client
        switch (version)
        {
            case 1:
                Log.Information("ClientFactory", $"Version 1 of the ListenWebSocketClient is being deprecated in the next major version.");
                Log.Information("ClientFactory", $"Transition to the latest version at your earliest convenience.");
                return new ListenV1.Client(apiKey, options);
            case 2:
                return new ListenV2.Client(apiKey, options);
            default:
                throw new ArgumentException("Invalid version", nameof(version));
        }
    }

    /// <summary>
    /// This method allows you to create an ManageClient with a specific version of the client.
    /// TODO: this should be revisited at a later time. Opportunity to use reflection to get the type of the client
    /// </summary>
    public static object CreateManageClient(int version, string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null)
    {
        // Currently only a single version of the ManageClient exists
        return new ManageClient(apiKey, options, httpId);
    }

    /// <summary>
    /// This method allows you to create an SelfHostedClient with a specific version of the client.
    /// TODO: this should be revisited at a later time. Opportunity to use reflection to get the type of the client
    /// </summary>
    public static object CreateSelfHostedClient(int version, string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null)
    {
        // Currently only a single version of the SelfHostedClient exists
        return new SelfHostedClient(apiKey, options, httpId);
    }

    /// <summary>
    /// This method allows you to create an SpeakRESTClient with a specific version of the client.
    /// TODO: this should be revisited at a later time. Opportunity to use reflection to get the type of the client
    /// </summary>
    public static object CreateSpeakRESTClient(int version, string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null)
    {
        // Currently only a single version of the SpeakRESTClient exists
        return new SpeakRESTClient(apiKey, options, httpId);
    }

    /// <summary>
    /// This method allows you to create an SpeakWebSocketClient with a specific version of the client.
    /// TODO: this should be revisited at a later time. Opportunity to use reflection to get the type of the client
    /// </summary>
    public static object CreateSpeakWebSocketClient(int version, string apiKey = "", DeepgramHttpClientOptions? options = null)
    {
        // at some point this needs to be changed to use reflection to get the type of the client
        switch (version)
        {
            case 1:
                Log.Information("ClientFactory", $"Version 1 of the ListenWebSocketClient is being deprecated in the next major version.");
                Log.Information("ClientFactory", $"Transition to the latest version at your earliest convenience.");
                return new SpeakV1.Client(apiKey, options);
            case 2:
                return new SpeakV2.Client(apiKey, options);
            default:
                throw new ArgumentException("Invalid version", nameof(version));
        }
    }

    ///
    /// The functions below are deprecated and will be removed in the next major version. You should not use
    /// these functions for new projects. They are only here to support older projects that are using these functions.
    /// 

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
    public static V1.ILiveClient CreateLiveClient(string apiKey = "", DeepgramWsClientOptions? options = null)
    {
        return new LiveClient(apiKey, options);
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
    public static V1.ISpeakClient CreateSpeakClient(string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null)
    {
        return new SpeakClient(apiKey, options, httpId);
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
    public static V1.IPreRecordedClient CreatePreRecordedClient(string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null)
    {
        return new PreRecordedClient(apiKey, options, httpId);
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
    public static V1.IOnPremClient CreateOnPremClient(string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null)
    {
        return new OnPremClient(apiKey, options, httpId);
    }
}
