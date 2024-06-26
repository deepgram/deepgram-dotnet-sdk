// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Speak.v1.REST;

namespace Deepgram.Clients.Interfaces.v1;

/// <summary>
/// Implements version 1 of the Speak Client.
/// </summary>
/// <param name="apiKey">Required DeepgramApiKey</param>
/// <param name="deepgramClientOptions"><see cref="DeepgramHttpClientOptions"/> for HttpClient Configuration</param>
public interface ISpeakRESTClient
{
    #region NoneCallBacks
    /// <summary>
    /// Speaks a file using the provided stream
    /// </summary>
    /// <param name="source">file is the form of a stream <see cref="Stream"/></param>
    /// <param name="speakSchema">Options for the transcription <see cref="SpeakSchema"/></param>
    /// <returns><see cref="SyncResponse"/></returns>
    public Task<SyncResponse> ToStream(TextSource source, SpeakSchema? speakSchema, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    public Task<SyncResponse> ToFile(TextSource source, string filename, SpeakSchema? speakSchema, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);
    #endregion

    #region  CallBack Methods
    /// <summary>
    /// Speaks a file using the provided byte array and providing a CallBack
    /// </summary>
    /// <param name="source">file is the form of a byte[]</param>  
    /// <param name="callBack">CallBack url</param>    
    /// <param name="speakSchema">Options for the transcription<see cref="SpeakSchema"></param>
    /// <returns><see cref="AsyncResponse"/></returns>
    public Task<AsyncResponse> StreamCallBack(TextSource source, string? callBack, SpeakSchema? speakSchema, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);
    #endregion
}
