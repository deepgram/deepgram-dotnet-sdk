// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Analyze.v1;
using Deepgram.Models.Authenticate.v1;

namespace Deepgram;

/// <summary>
///  Client containing methods for interacting with Read API's
/// </summary>
/// <param name="apiKey">Required DeepgramApiKey</param>
/// <param name="deepgramClientOptions"><see cref="DeepgramClientOptions"/> for HttpClient Configuration</param>
public class ReadClient(string apiKey, DeepgramClientOptions? deepgramClientOptions = null)
    : AbstractRestClient(apiKey, deepgramClientOptions)
{
    #region NoneCallBacks
    /// <summary>
    ///  Transcribe a file by providing a url 
    /// </summary>
    /// <param name="source">Url to the file that is to be transcribed <see cref="UrlSource"></param>
    /// <param name="analyzeSchema">Options for the transcription <see cref="AnalyzeSchema"/></param>
    /// <returns><see cref="SyncResponse"/></returns>
    public async Task<SyncResponse> TranscribeUrl(UrlSource source, AnalyzeSchema? analyzeSchema, CancellationToken cancellationToken = default)
    {
        VerifyNoCallBack(nameof(TranscribeUrl), analyzeSchema);
        var stringedOptions = QueryParameterUtil.GetParameters(analyzeSchema);
        return await PostAsync<SyncResponse>(
            $"{UriSegments.ANALYZE}?{stringedOptions}",
            RequestContentUtil.CreatePayload(source), cancellationToken);
    }
    /// <summary>
    /// Transcribes a file using the provided byte array
    /// </summary>
    /// <param name="source">file is the form of a byte[]</param>
    /// <param name="analyzeSchema">Options for the transcription <see cref="AnalyzeSchema"/></param>
    /// <returns><see cref="SyncResponse"/></returns>
    public async Task<SyncResponse> TranscribeFile(byte[] source, AnalyzeSchema? analyzeSchema, CancellationToken cancellationToken = default)
    {
        VerifyNoCallBack(nameof(TranscribeFile), analyzeSchema);
        var stringedOptions = QueryParameterUtil.GetParameters(analyzeSchema);
        var stream = new MemoryStream();
        stream.Write(source, 0, source.Length);
        return await PostAsync<SyncResponse>(
            $"{UriSegments.ANALYZE}?{stringedOptions}",
            RequestContentUtil.CreateStreamPayload(stream), cancellationToken);
    }

    /// <summary>
    /// Transcribes a file using the provided stream
    /// </summary>
    /// <param name="source">file is the form of a stream <see cref="Stream"/></param>
    /// <param name="analyzeSchema">Options for the transcription <see cref="AnalyzeSchema"/></param>
    /// <returns><see cref="SyncResponse"/></returns>
    public async Task<SyncResponse> TranscribeFile(Stream source, AnalyzeSchema? analyzeSchema, CancellationToken cancellationToken = default)
    {
        VerifyNoCallBack(nameof(TranscribeFile), analyzeSchema);
        var stringedOptions = QueryParameterUtil.GetParameters(analyzeSchema);
        return await PostAsync<SyncResponse>(
            $"{UriSegments.ANALYZE}?{stringedOptions}",
            RequestContentUtil.CreateStreamPayload(source), cancellationToken);
    }

    #endregion

    #region  CallBack Methods
    /// <summary>
    /// Transcribes a file using the provided byte array and providing a CallBack
    /// </summary>
    /// <param name="source">file is the form of a byte[]</param>  
    /// <param name="callBack">CallBack url</param>    
    /// <param name="analyzeSchema">Options for the transcription<see cref="AnalyzeSchema"></param>
    /// <returns><see cref="AsyncResponse"/></returns>
    public async Task<AsyncResponse> TranscribeFileCallBack(byte[] source, string? callBack, AnalyzeSchema? analyzeSchema, CancellationToken cancellationToken = default)
    {
        VerifyOneCallBackSet(nameof(TranscribeFileCallBack), callBack, analyzeSchema);

        if (callBack != null)
            analyzeSchema.CallBack = callBack;
        var stringedOptions = QueryParameterUtil.GetParameters(analyzeSchema);
        var stream = new MemoryStream();
        stream.Write(source, 0, source.Length);
        return await PostAsync<AsyncResponse>(
            $"{UriSegments.ANALYZE}?{stringedOptions}",
            RequestContentUtil.CreateStreamPayload(stream), cancellationToken);
    }

    /// <summary>
    /// Transcribes a file using the provided stream and providing a CallBack
    /// </summary>
    /// <param name="source">file is the form of a stream <see cref="Stream"></param>  
    /// <param name="callBack">CallBack url</param>    
    /// <param name="analyzeSchema">Options for the transcription<see cref="AnalyzeSchema"></param>
    /// <returns><see cref="AsyncResponse"/></returns>
    public async Task<AsyncResponse> TranscribeFileCallBack(Stream source, string? callBack, AnalyzeSchema? analyzeSchema, CancellationToken cancellationToken = default)
    {
        VerifyOneCallBackSet(nameof(TranscribeFileCallBack), callBack, analyzeSchema);
        if (callBack != null)
            analyzeSchema.CallBack = callBack;
        var stringedOptions = QueryParameterUtil.GetParameters(analyzeSchema);
        return await PostAsync<AsyncResponse>(
            $"{UriSegments.ANALYZE}?{stringedOptions}",
            RequestContentUtil.CreateStreamPayload(source), cancellationToken);
    }

    /// <summary>
    /// Transcribe a file by providing a url and a CallBack
    /// </summary>
    /// <param name="source">Url to the file that is to be transcribed <see cref="UrlSource"/></param>
    /// <param name="callBack">CallBack url</param>    
    /// <param name="analyzeSchema">Options for the transcription<see cref="AnalyzeSchema"></param>
    /// <returns><see cref="AsyncResponse"/></returns>
    public async Task<AsyncResponse> TranscribeUrlCallBack(UrlSource source, string? callBack, AnalyzeSchema? analyzeSchema, CancellationToken cancellationToken = default)
    {
        VerifyOneCallBackSet(nameof(TranscribeUrlCallBack), callBack, analyzeSchema);

        if (callBack != null)
            analyzeSchema.CallBack = callBack;
        var stringedOptions = QueryParameterUtil.GetParameters(analyzeSchema);
        return await PostAsync<AsyncResponse>(
            $"{UriSegments.ANALYZE}?{stringedOptions}",
            RequestContentUtil.CreatePayload(source), cancellationToken);
    }
    #endregion

    #region CallbackChecks
    private void VerifyNoCallBack(string method, AnalyzeSchema? analyzeSchema)
    {
        if (analyzeSchema != null && analyzeSchema.CallBack != null)
            throw new ArgumentException($"CallBack cannot be provided as schema option to a synchronous transcription when calling {method}. Use {nameof(TranscribeFileCallBack)} or {nameof(TranscribeUrlCallBack)}");
    }

    private void VerifyOneCallBackSet(string callingMethod, string? callBack, AnalyzeSchema? analyzeSchema)
    {

        if (analyzeSchema.CallBack == null && callBack == null)
        { //check if no CallBack set in either callBack parameter or AnalyzeSchema
            var ex = new ArgumentException($"Either provide a CallBack url or set AnalyzeSchema.CallBack.  If no CallBack needed either {nameof(TranscribeUrl)} or {nameof(TranscribeFile)}");
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
