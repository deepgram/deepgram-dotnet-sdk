// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Listen.v1.REST;

namespace Deepgram.Clients.Interfaces.v1;

/// <summary>
/// Implements version 1 of the Analyze Client.
/// </summary>
/// <param name="apiKey">Required DeepgramApiKey</param>
/// <param name="deepgramClientOptions"><see cref="DeepgramHttpClientOptions"/> for HttpClient Configuration</param>
public interface IListenRESTClient

{
    #region NoneCallBacks
    /// <summary>
    ///  Transcribe a file by providing a url 
    /// </summary>
    /// <param name="source">Url to the file that is to be transcribed <see cref="UrlSource"></param>
    /// <param name="prerecordedSchema">Options for the transcription <see cref="PreRecordedSchema"/></param>
    /// <returns><see cref="SyncResponse"/></returns>
    public Task<SyncResponse> TranscribeUrl(UrlSource source, PreRecordedSchema? prerecordedSchema,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Transcribes a file using the provided byte array
    /// </summary>
    /// <param name="source">file is the form of a byte[]</param>
    /// <param name="prerecordedSchema">Options for the transcription <see cref="PreRecordedSchema"/></param>
    /// <returns><see cref="SyncResponse"/></returns>
    public Task<SyncResponse> TranscribeFile(byte[] source, PreRecordedSchema? prerecordedSchema,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Transcribes a file using the provided stream
    /// </summary>
    /// <param name="source">file is the form of a streasm <see cref="Stream"/></param>
    /// <param name="prerecordedSchema">Options for the transcription <see cref="PreRecordedSchema"/></param>
    /// <returns><see cref="SyncResponse"/></returns>
    public Task<SyncResponse> TranscribeFile(Stream source, PreRecordedSchema? prerecordedSchema,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null);
    #endregion

    #region  CallBack Methods
    /// <summary>
    /// Transcribes a file using the provided byte array and providing a CallBack
    /// </summary>
    /// <param name="source">file is the form of a byte[]</param>  
    /// <param name="callBack">CallBack url</param>    
    /// <param name="prerecordedSchema">Options for the transcription<see cref="PreRecordedSchema"></param>
    /// <returns><see cref="AsyncResponse"/></returns>
    public Task<AsyncResponse> TranscribeFileCallBack(byte[] source, string? callBack, PreRecordedSchema? prerecordedSchema,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null);

    /// <summary>
    /// Transcribes a file using the provided stream and providing a CallBack
    /// </summary>
    /// <param name="source">file is the form of a stream <see cref="Stream"></param>  
    /// <param name="callBack">CallBack url</param>    
    /// <param name="prerecordedSchema">Options for the transcription<see cref="PreRecordedSchema"></param>
    /// <returns><see cref="AsyncResponse"/></returns>
    public Task<AsyncResponse> TranscribeFileCallBack(Stream source, string? callBack, PreRecordedSchema? prerecordedSchema,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null);

    /// <summary>
    /// Transcribe a file by providing a url and a CallBack
    /// </summary>
    /// <param name="source">Url to the file that is to be transcribed <see cref="UrlSource"/></param>
    /// <param name="callBack">CallBack url</param>    
    /// <param name="prerecordedSchema">Options for the transcription<see cref="PreRecordedSchema"></param>
    /// <returns><see cref="AsyncResponse"/></returns>
    public Task<AsyncResponse> TranscribeUrlCallBack(UrlSource source, string? callBack, PreRecordedSchema? prerecordedSchema,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null);
    #endregion 
}
