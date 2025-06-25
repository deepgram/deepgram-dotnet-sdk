// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Authenticate.v1;

public interface IDeepgramClientOptions
{
    /*****************************/
    // General Options
    /*****************************/
    /// <summary>
    /// Deepgram API KEY
    /// </summary>
    public string ApiKey { get; }

    /// <summary>
    /// Deepgram Access Token (OAuth 2.0 compliant)
    /// </summary>
    public string AccessToken { get; }

    /// <summary>
    /// BaseAddress of the server :defaults to api.deepgram.com
    /// no need to attach the protocol it will be added internally
    /// </summary>
    public string BaseAddress { get; }

    /// <summary>
    /// Api endpoint version
    /// </summary>
    public string APIVersion { get; }

    /// <summary>
    /// Global headers to always be added to the request
    /// </summary>
    public Dictionary<string, string> Headers { get; }

    /// <summary>
    /// Global addons to always be added to the request
    /// </summary>
    public Dictionary<string, string> Addons { get; }

    /*****************************/
    // Prerecorded
    /*****************************/

    /*****************************/
    // Live
    /*****************************/
    /// <summary>
    /// Enable sending KeepAlives for Streaming
    /// </summary>
    public bool KeepAlive { get; }

    /// <summary>
    /// <summary>
    /// Sets the interval for automatic flushing of replies in Listen Streaming
    /// </summary>
    public decimal AutoFlushReplyDelta { get; }
    /// <summary>
    /// Based on the options set, do we want to inspect the Listen Messages. If yes, then return true.
    /// </summary>
    public bool InspectListenMessage();

    /// <summary>
    /// Based on the options set, do we want to inspect the Speak Messages. If yes, then return true.
    /// </summary>
    public bool InspectSpeakMessage();

    /*****************************/
    // OnPrem
    /*****************************/
    /// <summary>
    /// Enable when using OnPrem mode
    /// </summary>
    public bool OnPrem { get; }

    /*****************************/
    // Manage
    /*****************************/

    /*****************************/
    // Analyze
    /*****************************/

    /*****************************/
    // Speak
    /*****************************/

    /// <summary>
    /// Sets the interval for automatic flushing in Speak Streaming
    /// </summary>
    public decimal AutoFlushSpeakDelta { get; }

    /*****************************/
    // Dynamic Authentication Methods
    /*****************************/

    /// <summary>
    /// Sets the API Key for authentication (clears AccessToken)
    /// </summary>
    /// <param name="apiKey">The API Key to use</param>
    void SetApiKey(string apiKey);

    /// <summary>
    /// Sets the Access Token for authentication (clears ApiKey)
    /// </summary>
    /// <param name="accessToken">The Access Token to use</param>
    void SetAccessToken(string accessToken);

    /// <summary>
    /// Clears all authentication credentials
    /// </summary>
    void ClearCredentials();
}
