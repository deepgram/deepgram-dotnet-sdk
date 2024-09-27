// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Analyze.v1;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Clients.Interfaces.v1;

namespace Deepgram.Clients.Analyze.v1;

/// <summary>
/// Implements version 1 of the Analyze Client.
/// </summary>
/// <param name="apiKey">Required DeepgramApiKey</param>
/// <param name="deepgramClientOptions"><see cref="DeepgramHttpClientOptions"/> for HttpClient Configuration</param>
public class Client(string? apiKey = null, IDeepgramClientOptions? deepgramClientOptions = null, string? httpId = null)
    : AbstractRestClient(apiKey, deepgramClientOptions, httpId), IAnalyzeClient
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
        Log.Verbose("AnalyzeClient.AnalyzeUrl", "ENTER");
        Log.Information("AnalyzeUrl", $"source: {source}");
        Log.Information("AnalyzeUrl", $"analyzeSchema:\n{JsonSerializer.Serialize(analyzeSchema, JsonSerializeOptions.DefaultOptions)}");

        VerifyNoCallBack(nameof(AnalyzeUrl), analyzeSchema);

        var uri = GetUri(_options, UriSegments.READ);
        var result = await PostAsync<UrlSource, AnalyzeSchema, SyncResponse>(
            uri, analyzeSchema, source, cancellationToken, addons, headers
            );

        Log.Information("AnalyzeUrl", $"{uri} Succeeded");
        Log.Debug("AnalyzeUrl", $"result: {result}");
        Log.Verbose("AnalyzeClient.AnalyzeUrl", "LEAVE");

        return result;
    }

    /// <summary>
    ///  Analyze by providing text 
    /// </summary>
    /// <param name="source">Text that is to be analyzed <see cref="TextSource"></param>
    /// <param name="analyzeSchema">Options for the transcription <see cref="AnalyzeSchema"/></param>
    /// <returns><see cref="SyncResponse"/></returns>
    public async Task<SyncResponse> AnalyzeText(TextSource source, AnalyzeSchema? analyzeSchema, CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("AnalyzeClient.AnalyzeText", "ENTER");
        Log.Information("AnalyzeText", $"source: {source}");
        Log.Information("AnalyzeText", $"analyzeSchema:\n{JsonSerializer.Serialize(analyzeSchema, JsonSerializeOptions.DefaultOptions)}");

        VerifyNoCallBack(nameof(AnalyzeText), analyzeSchema);

        var uri = GetUri(_options, UriSegments.READ);
        var result = await PostAsync<TextSource, AnalyzeSchema, SyncResponse>(
            uri, analyzeSchema, source, cancellationToken, addons, headers
            );

        Log.Information("AnalyzeText", $"{uri} Succeeded");
        Log.Debug("AnalyzeText", $"result: {result}");
        Log.Verbose("AnalyzeClient.AnalyzeText", "LEAVE");

        return result;
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
        Log.Verbose("AnalyzeClient.AnalyzeFile", "ENTER");
        Log.Information("AnalyzeFile", $"source: {source}");
        Log.Information("AnalyzeFile", $"analyzeSchema:\n{analyzeSchema}");

        VerifyNoCallBack(nameof(AnalyzeFile), analyzeSchema);

        var uri = GetUri(_options, UriSegments.READ);
        var result = await PostAsync<Stream, AnalyzeSchema, SyncResponse>(
            uri, analyzeSchema, source, cancellationToken, addons, headers
        );

        Log.Information("AnalyzeFile", $"{uri} Succeeded");
        Log.Debug("AnalyzeFile", $"result: {result}");
        Log.Verbose("AnalyzeClient.AnalyzeFile", "LEAVE");

        return result;
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
        Log.Verbose("AnalyzeClient.AnalyzeFileCallBack", "ENTER");
        Log.Information("AnalyzeFileCallBack", $"source: {source}");
        Log.Information("AnalyzeFileCallBack", $"callBack: {callBack}");
        Log.Information("AnalyzeFileCallBack", $"analyzeSchema:\n{analyzeSchema}");

        VerifyOneCallBackSet(nameof(AnalyzeFileCallBack), callBack, analyzeSchema);
        if (callBack != null)
            analyzeSchema.CallBack = callBack;

        var uri = GetUri(_options, UriSegments.READ);
        var result = await PostAsync<Stream, AnalyzeSchema, AsyncResponse>(
            uri, analyzeSchema, source, cancellationToken, addons, headers
            );

        Log.Information("AnalyzeFileCallBack", $"{uri} Succeeded");
        Log.Debug("AnalyzeFileCallBack", $"result: {result}");
        Log.Verbose("AnalyzeClient.AnalyzeFileCallBack", "LEAVE");

        return result;
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
        Log.Verbose("AnalyzeClient.AnalyzeUrlCallBack", "ENTER");
        Log.Information("AnalyzeUrlCallBack", $"source: {source}");
        Log.Information("AnalyzeUrlCallBack", $"callBack: {callBack}");
        Log.Information("AnalyzeUrlCallBack", $"analyzeSchema:\n{JsonSerializer.Serialize(analyzeSchema, JsonSerializeOptions.DefaultOptions)}");

        VerifyOneCallBackSet(nameof(AnalyzeUrlCallBack), callBack, analyzeSchema);
        if (callBack != null)
            analyzeSchema.CallBack = callBack;

        var uri = GetUri(_options, UriSegments.READ);
        var result = await PostAsync<UrlSource, AnalyzeSchema, AsyncResponse>(
            uri, analyzeSchema, source, cancellationToken, addons, headers
            );

        Log.Information("AnalyzeUrlCallBack", $"{uri} Succeeded");
        Log.Debug("AnalyzeUrlCallBack", $"result: {result}");
        Log.Verbose("AnalyzeClient.AnalyzeUrlCallBack", "LEAVE");

        return result;
    }

    /// <summary>
    /// Analyze by providing text and a CallBack
    /// </summary>
    /// <param name="source">Text that is to be analyzed <see cref="UrlSource"/></param>
    /// <param name="callBack">CallBack url</param>    
    /// <param name="analyzeSchema">Options for the transcription<see cref="AnalyzeSchema"></param>
    /// <returns><see cref="AsyncResponse"/></returns>
    public async Task<AsyncResponse> AnalyzeTextCallBack(TextSource source, string? callBack, AnalyzeSchema? analyzeSchema, CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("AnalyzeClient.AnalyzeTextCallBack", "ENTER");
        Log.Information("AnalyzeTextCallBack", $"source: {source}");
        Log.Information("AnalyzeTextCallBack", $"callBack: {callBack}");
        Log.Information("AnalyzeTextCallBack", $"analyzeSchema:\n{JsonSerializer.Serialize(analyzeSchema, JsonSerializeOptions.DefaultOptions)}");

        VerifyOneCallBackSet(nameof(AnalyzeTextCallBack), callBack, analyzeSchema);
        if (callBack != null)
            analyzeSchema.CallBack = callBack;

        var uri = GetUri(_options, UriSegments.READ);
        var result = await PostAsync<TextSource, AnalyzeSchema, AsyncResponse>(
            uri, analyzeSchema, source, cancellationToken, addons, headers
            );

        Log.Information("AnalyzeTextCallBack", $"{uri} Succeeded");
        Log.Debug("AnalyzeTextCallBack", $"result: {result}");
        Log.Verbose("AnalyzeClient.AnalyzeTextCallBack", "LEAVE");

        return result;
    }
    #endregion

    #region CallbackChecks
    private void VerifyNoCallBack(string method, AnalyzeSchema? analyzeSchema)
    {
        Log.Debug("VerifyNoCallBack", $"method: {method}");

        if (analyzeSchema != null && analyzeSchema.CallBack != null)
        {
            var exStr = $"CallBack cannot be provided as schema option to a synchronous transcription when calling {method}. Use {nameof(AnalyzeFileCallBack)} or {nameof(AnalyzeUrlCallBack)}";
            Log.Error("VerifyNoCallBack", $"Exception: {exStr}");
            throw new ArgumentException(exStr);
        }
    }

    private void VerifyOneCallBackSet(string method, string? callBack, AnalyzeSchema? analyzeSchema)
    {
        Log.Debug("VerifyOneCallBackSet", $"method: {method}");

        if (analyzeSchema.CallBack == null && callBack == null)
        {
            //check if no CallBack set in either callBack parameter or AnalyzeSchema
            var exStr = $"Either provide a CallBack url or set AnalyzeSchema.CallBack.  If no CallBack needed either {nameof(AnalyzeUrl)} or {nameof(AnalyzeFile)}";
            Log.Error("VerifyNoCallBack", $"Exception: {exStr}");
            throw new ArgumentException(exStr);
        }
        else if (!string.IsNullOrEmpty(analyzeSchema.CallBack) && !string.IsNullOrEmpty(callBack))
        {
            //check that only one CallBack is set in either callBack parameter or AnalyzeSchema
            var exStr = "CallBack should be set in either the CallBack parameter or AnalyzeSchema.CallBack not in both.";
            Log.Error("VerifyNoCallBack", $"Exceptions: {exStr}");
            throw new ArgumentException(exStr);
        }
    }
    #endregion 
}
