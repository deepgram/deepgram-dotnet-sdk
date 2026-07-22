// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Flux.Speak.REST;

namespace Deepgram.Clients.Interfaces.v2;

/// <summary>
/// (PREVIEW) Implements the Flux (v2 speak) batch text-to-speech REST Client (POST /v2/speak):
/// synthesize a complete block of text into a single audio response.
/// </summary>
public interface IFluxSpeakRESTClient
{
    #region NoneCallBacks
    /// <summary>
    /// Synthesizes the supplied text and returns the audio as a stream plus response metadata.
    /// </summary>
    /// <param name="source">The text to synthesize <see cref="TextSource"/></param>
    /// <param name="speakSchema">Query options for the request <see cref="SpeakSchema"/></param>
    /// <returns><see cref="SyncResponse"/></returns>
    public Task<SyncResponse> ToStream(TextSource source, SpeakSchema? speakSchema, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Synthesizes the supplied text and writes the audio to the given file.
    /// </summary>
    public Task<SyncResponse> ToFile(TextSource source, string filename, SpeakSchema? speakSchema, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);
    #endregion

    #region CallBack Methods
    /// <summary>
    /// Synthesizes the supplied text asynchronously via a callback URL, returning an
    /// acknowledgement ({request_id}); the audio is delivered to the callback URL.
    /// </summary>
    /// <param name="source">The text to synthesize <see cref="TextSource"/></param>
    /// <param name="callBack">Callback URL</param>
    /// <param name="speakSchema">Query options for the request <see cref="SpeakSchema"/></param>
    /// <returns><see cref="AsyncResponse"/></returns>
    public Task<AsyncResponse> StreamCallBack(TextSource source, string? callBack, SpeakSchema? speakSchema, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);
    #endregion
}
