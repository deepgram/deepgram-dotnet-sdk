// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Analyze.v1;

namespace Deepgram.Clients.Interfaces.v1;

public interface IAnalyzeClient
{
    #region NoneCallBacks
    /// <summary>
    ///  Analyze a file by providing a url 
    /// </summary>
    /// <param name="source">Url to the file that is to be analyzed <see cref="UrlSource"></param>
    /// <param name="analyzeSchema">Options for the transcription <see cref="AnalyzeSchema"/></param>
    /// <returns><see cref="SyncResponse"/></returns>
    public Task<SyncResponse> AnalyzeUrl(UrlSource source, AnalyzeSchema? analyzeSchema, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    ///  Analyze by providing text 
    /// </summary>
    /// <param name="source">Text that is to be analyzed <see cref="TextSource"></param>
    /// <param name="analyzeSchema">Options for the transcription <see cref="AnalyzeSchema"/></param>
    /// <returns><see cref="SyncResponse"/></returns>
    public Task<SyncResponse> AnalyzeText(TextSource source, AnalyzeSchema? analyzeSchema, CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Analyzes a file using the provided byte array
    /// </summary>
    /// <param name="source">file is the form of a byte[]</param>
    /// <param name="analyzeSchema">Options for the transcription <see cref="AnalyzeSchema"/></param>
    /// <returns><see cref="SyncResponse"/></returns>
    public Task<SyncResponse> AnalyzeFile(byte[] source, AnalyzeSchema? analyzeSchema, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);

    /// <summary>
    /// Analyzes a file using the provided stream
    /// </summary>
    /// <param name="source">file is the form of a stream <see cref="Stream"/></param>
    /// <param name="analyzeSchema">Options for the transcription <see cref="AnalyzeSchema"/></param>
    /// <returns><see cref="SyncResponse"/></returns>
    public Task<SyncResponse> AnalyzeFile(Stream source, AnalyzeSchema? analyzeSchema, CancellationTokenSource? cancellationToken = default,
        Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);
    #endregion

    #region  CallBack Methods
    /// <summary>
    /// Analyzes a file using the provided byte array and providing a CallBack
    /// </summary>
    /// <param name="source">file is the form of a byte[]</param>  
    /// <param name="callBack">CallBack url</param>    
    /// <param name="analyzeSchema">Options for the transcription<see cref="AnalyzeSchema"></param>
    /// <returns><see cref="AsyncResponse"/></returns>
    public Task<AsyncResponse> AnalyzeFileCallBack(byte[] source, string? callBack, AnalyzeSchema? analyzeSchema,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null);

    /// <summary>
    /// Analyzes a file using the provided stream and providing a CallBack
    /// </summary>
    /// <param name="source">file is the form of a stream <see cref="Stream"></param>  
    /// <param name="callBack">CallBack url</param>    
    /// <param name="analyzeSchema">Options for the transcription<see cref="AnalyzeSchema"></param>
    /// <returns><see cref="AsyncResponse"/></returns>
    public Task<AsyncResponse> AnalyzeFileCallBack(Stream source, string? callBack, AnalyzeSchema? analyzeSchema,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null);

    /// <summary>
    /// Analyze a file by providing a url and a CallBack
    /// </summary>
    /// <param name="source">Url to the file that is to be analyzed <see cref="UrlSource"/></param>
    /// <param name="callBack">CallBack url</param>    
    /// <param name="analyzeSchema">Options for the transcription<see cref="AnalyzeSchema"></param>
    /// <returns><see cref="AsyncResponse"/></returns>
    public Task<AsyncResponse> AnalyzeUrlCallBack(UrlSource source, string? callBack, AnalyzeSchema? analyzeSchema,
        CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null);

    // <summary>
    /// Analyze by providing text and a CallBack
    /// </summary>
    /// <param name="source">Text that is to be analyzed <see cref="UrlSource"/></param>
    /// <param name="callBack">CallBack url</param>    
    /// <param name="analyzeSchema">Options for the transcription<see cref="AnalyzeSchema"></param>
    /// <returns><see cref="AsyncResponse"/></returns>
    public Task<AsyncResponse> AnalyzeTextCallBack(TextSource source, string? callBack, AnalyzeSchema? analyzeSchema, CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null);
    #endregion
}
