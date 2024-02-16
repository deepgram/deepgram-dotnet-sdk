// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Authenticate.v1;

/// <summary>
/// Configuration for the Deepgram client
/// </summary>
public class DeepgramClientOptions
{
    /*****************************/
    // General Options
    /*****************************/
    /// <summary>
    /// BaseAddress of the server :defaults to api.deepgram.com
    /// no need to attach the protocol it will be added internally
    /// </summary>
    public string BaseAddress { get; set; } = Defaults.DEFAULT_URI;

    /// <summary>
    /// Api endpoint version
    /// </summary>
    public string APIVersion { get; set; } = "v1";

    /// <summary>
    /// Additional headers 
    /// </summary>
    public Dictionary<string, string> Headers { get; set; }

    /// <summary>
    /// Enable when using OnPrem mode
    /// </summary>
    public bool OnPrem { get; set; } = false;

    /*****************************/
    // Prerecorded
    /*****************************/

    /*****************************/
    // Live
    /*****************************/
    /// <summary>
    /// Enable sending KeepAlives for Streaming
    /// </summary>
    public bool EnableKeepAlive { get; set; } = false;

    /*****************************/
    // OnPrem
    /*****************************/

    /*****************************/
    // Manage
    /*****************************/

    /*****************************/
    // Analyze
    /*****************************/
}
