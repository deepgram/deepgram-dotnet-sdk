// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Analyze.v1;
using Deepgram.Models.Authenticate.v1;

namespace Deepgram.Clients.Analyze.v1;

/// <summary>
/// Implements version 1 of the Analyze Client.
/// </summary>
/// <param name="apiKey">Required DeepgramApiKey</param>
/// <param name="deepgramClientOptions"><see cref="DeepgramHttpClientOptions"/> for HttpClient Configuration</param>
public class Client(string? apiKey = null, DeepgramHttpClientOptions? deepgramClientOptions = null)
    : AbstractRestClient(apiKey, deepgramClientOptions)
{
    #region NoneCallBacks
    /// <summary>
    ///  Analyze a file by providing a url 
    /// </summary>
    /// <param name="source">Url to the file that is to be analyzed <see cref="UrlSource"></param>
    /// <param name="analyzeSchema">Options for the transcription <see cref="AnalyzeSchema"/></param>
    /// <returns><see cref="SyncResponse"/></returns>
    public async Task<SyncResponse> AnalyzeUrl(UrlSource source, AnalyzeSchema? analyzeSchema, CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        VerifyNoCallBack(nameof(AnalyzeUrl), analyzeSchema);

        return await PostAsync<UrlSource, AnalyzeSchema, SyncResponse>(
            GetUri(_options, UriSegments.READ), analyzeSchema, source, cancellationToken, addons, headers
            );
    }


    /// <summary>
    /// Analyzes a file using the provided byte array
    /// </summary>
    /// <param name="source">file is the form of a byte[]</param>
    /// <param name="analyzeSchema">Options for the transcription <see cref="AnalyzeSchema"/></param>
    /// <returns><see cref="SyncResponse"/></returns>
    public async Task<SyncResponse> AnalyzeFile(byte[] source, AnalyzeSchema? analyzeSchema, CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        MemoryStream stream = new MemoryStream(source);
        return await AnalyzeFile(stream, analyzeSchema, cancellationToken, addons, headers);
    }

    /// <summary>
    /// Analyzes a file using the provided stream
    /// </summary>
    /// <param name="source">file is the form of a stream <see cref="Stream"/></param>
    /// <param name="analyzeSchema">Options for the transcription <see cref="AnalyzeSchema"/></param>
    /// <returns><see cref="SyncResponse"/></returns>
    public async Task<SyncResponse> AnalyzeFile(Stream source, AnalyzeSchema? analyzeSchema, CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        VerifyNoCallBack(nameof(AnalyzeFile), analyzeSchema);

        return await PostAsync<Stream, AnalyzeSchema, SyncResponse>(
            GetUri(_options, UriSegments.READ), analyzeSchema, source, cancellationToken, addons, headers
            );
    }

    #endregion

    #region  CallBack Methods
    /// <summary>
    /// Analyzes a file using the provided byte array and providing a CallBack
    /// </summary>
    /// <param name="source">file is the form of a byte[]</param>  
    /// <param name="callBack">CallBack url</param>    
    /// <param name="analyzeSchema">Options for the transcription<see cref="AnalyzeSchema"></param>
    /// <returns><see cref="AsyncResponse"/></returns>
    public async Task<AsyncResponse> AnalyzeFileCallBack(byte[] source, string? callBack, AnalyzeSchema? analyzeSchema, CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        MemoryStream stream = new MemoryStream(source);
        return await AnalyzeFileCallBack(stream, callBack, analyzeSchema, cancellationToken, addons, headers);
    }

    /// <summary>
    /// Analyzes a file using the provided stream and providing a CallBack
    /// </summary>
    /// <param name="source">file is the form of a stream <see cref="Stream"></param>  
    /// <param name="callBack">CallBack url</param>    
    /// <param name="analyzeSchema">Options for the transcription<see cref="AnalyzeSchema"></param>
    /// <returns><see cref="AsyncResponse"/></returns>
    public async Task<AsyncResponse> AnalyzeFileCallBack(Stream source, string? callBack, AnalyzeSchema? analyzeSchema, CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        VerifyOneCallBackSet(nameof(AnalyzeFileCallBack), callBack, analyzeSchema);
        if (callBack != null)
            analyzeSchema.CallBack = callBack;

        return await PostAsync<Stream, AnalyzeSchema, AsyncResponse>(
            GetUri(_options, UriSegments.READ), analyzeSchema, source, cancellationToken, addons, headers
            );
    }

    /// <summary>
    /// Analyze a file by providing a url and a CallBack
    /// </summary>
    /// <param name="source">Url to the file that is to be analyzed <see cref="UrlSource"/></param>
    /// <param name="callBack">CallBack url</param>    
    /// <param name="analyzeSchema">Options for the transcription<see cref="AnalyzeSchema"></param>
    /// <returns><see cref="AsyncResponse"/></returns>
    public async Task<AsyncResponse> AnalyzeUrlCallBack(UrlSource source, string? callBack, AnalyzeSchema? analyzeSchema, CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        VerifyOneCallBackSet(nameof(AnalyzeUrlCallBack), callBack, analyzeSchema);
        if (callBack != null)
            analyzeSchema.CallBack = callBack;

        return await PostAsync<UrlSource, AnalyzeSchema, AsyncResponse>(
            GetUri(_options, UriSegments.READ), analyzeSchema, source, cancellationToken, addons, headers
            );
    }
    #endregion

    #region CallbackChecks
    private void VerifyNoCallBack(string method, AnalyzeSchema? analyzeSchema)
    {
        // TODO: think about logging here based on coderabbit feedback
        if (analyzeSchema != null && analyzeSchema.CallBack != null)
            throw new ArgumentException($"CallBack cannot be provided as schema option to a synchronous transcription when calling {method}. Use {nameof(AnalyzeFileCallBack)} or {nameof(AnalyzeUrlCallBack)}");
    }

    private void VerifyOneCallBackSet(string callingMethod, string? callBack, AnalyzeSchema? analyzeSchema)
    {
        // TODO: think about logging here based on coderabbit feedback
        if (analyzeSchema.CallBack == null && callBack == null)
        { //check if no CallBack set in either callBack parameter or AnalyzeSchema
            var ex = new ArgumentException($"Either provide a CallBack url or set AnalyzeSchema.CallBack.  If no CallBack needed either {nameof(AnalyzeUrl)} or {nameof(AnalyzeFile)}");
            Log.Exception(_logger, $"While calling {callingMethod} no callback set", ex);
            throw ex;
        }
        else if (!string.IsNullOrEmpty(analyzeSchema.CallBack) && !string.IsNullOrEmpty(callBack))
        {
            //check that only one CallBack is set in either callBack parameter or AnalyzeSchema
            var ex = new ArgumentException("CallBack should be set in either the CallBack parameter or AnalyzeSchema.CallBack not in both.");
            Log.Exception(_logger, $"While calling {callingMethod}, callback set in both parameter and property", ex);
            throw ex;
        }
    }
    #endregion 
}
