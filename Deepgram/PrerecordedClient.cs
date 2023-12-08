using Deepgram.Records;
using Deepgram.Records.PreRecorded;

namespace Deepgram;

/// <summary>
/// Constructor to create a client for the Deepgram Prerecorded API
/// </summary>
/// <param name="deepgramClientOptions"><see cref="DeepgramClientOptions"/> for HttpClient Configuration</param>
/// <param name="httpClient"><see cref="HttpClient"/> for making Rest calls</param>
public sealed class PrerecordedClient(DeepgramClientOptions deepgramClientOptions, HttpClient httpClient)
    : AbstractRestClient(deepgramClientOptions, httpClient)
{
    readonly string _urlPrefix = $"/{Constants.API_VERSION}/{Constants.LISTEN}";
    #region NoneCallBacks
    /// <summary>
    ///  Transcribe a file by providing a url 
    /// </summary>
    /// <param name="source">Url to the file that is to be transcribed <see cref="UrlSource"></param>
    /// <param name="prerecordedSchema">Options for the transcription <see cref="PrerecordedSchema"/></param>
    /// <returns><see cref="SyncPrerecordedResponse"/></returns>
    public async Task<SyncPrerecordedResponse> TranscribeUrlAsync(UrlSource source, PrerecordedSchema? prerecordedSchema)
    {
        VerifyNoCallBack(nameof(TranscribeUrlAsync), prerecordedSchema);
        var stringedOptions = QueryParameterUtil.GetParameters(prerecordedSchema);
        return await PostAsync<SyncPrerecordedResponse>(
            $"{_urlPrefix}?{stringedOptions}",
            RequestContentUtil.CreatePayload(source));
    }


    /// <summary>
    /// Transcribes a file using the provided byte array
    /// </summary>
    /// <param name="source">file is the form of a byte[]</param>
    /// <param name="prerecordedSchema">Options for the transcription <see cref="PrerecordedSchema"/></param>
    /// <returns><see cref="SyncPrerecordedResponse"/></returns>
    public async Task<SyncPrerecordedResponse> TranscribeFileAsync(byte[] source, PrerecordedSchema? prerecordedSchema)
    {
        VerifyNoCallBack(nameof(TranscribeFileAsync), prerecordedSchema);
        var stringedOptions = QueryParameterUtil.GetParameters(prerecordedSchema);
        var stream = new MemoryStream();
        stream.Write(source, 0, source.Length);
        return await PostAsync<SyncPrerecordedResponse>(
            $"{_urlPrefix}?{stringedOptions}",
            RequestContentUtil.CreateStreamPayload(stream));
    }

    /// <summary>
    /// Transcribes a file using the provided stream
    /// </summary>
    /// <param name="source">file is the form of a stream <see cref="Stream"/></param>
    /// <param name="prerecordedSchema">Options for the transcription <see cref="PrerecordedSchema"/></param>
    /// <returns><see cref="SyncPrerecordedResponse"/></returns>
    public async Task<SyncPrerecordedResponse> TranscribeFileAsync(Stream source, PrerecordedSchema? prerecordedSchema)
    {
        VerifyNoCallBack(nameof(TranscribeFileAsync), prerecordedSchema);
        var stringedOptions = QueryParameterUtil.GetParameters(prerecordedSchema);
        return await PostAsync<SyncPrerecordedResponse>(
            $"{_urlPrefix}?{stringedOptions}",
            RequestContentUtil.CreateStreamPayload(source));
    }

    #endregion

    #region  CallBack Methods
    /// <summary>
    /// Transcribes a file using the provided byte array and providing a CallBack
    /// </summary>
    /// <param name="source">file is the form of a byte[]</param>  
    /// <param name="callBack">CallBack url</param>    
    /// <param name="prerecordedSchema">Options for the transcription<see cref="PrerecordedSchema"></param>
    /// <returns><see cref="AsyncPrerecordedResponse"/></returns>
    public async Task<AsyncPrerecordedResponse> TranscribeFileCallBackAsync(byte[] source, string? callBack, PrerecordedSchema? prerecordedSchema)
    {
        VerifyOneCallBackSet(nameof(TranscribeFileCallBackAsync), callBack, prerecordedSchema);

        if (callBack != null)
            prerecordedSchema.Callback = callBack;
        var stringedOptions = QueryParameterUtil.GetParameters(prerecordedSchema);
        var stream = new MemoryStream();
        stream.Write(source, 0, source.Length);
        return await PostAsync<AsyncPrerecordedResponse>(
            $"{_urlPrefix}?{stringedOptions}",
            RequestContentUtil.CreateStreamPayload(stream));
    }

    /// <summary>
    /// Transcribes a file using the provided stream and providing a CallBack
    /// </summary>
    /// <param name="source">file is the form of a stream <see cref="Stream"></param>  
    /// <param name="callBack">CallBack url</param>    
    /// <param name="prerecordedSchema">Options for the transcription<see cref="PrerecordedSchema"></param>
    /// <returns><see cref="AsyncPrerecordedResponse"/></returns>
    public async Task<AsyncPrerecordedResponse> TranscribeFileCallBackAsync(Stream source, string? callBack, PrerecordedSchema? prerecordedSchema)
    {
        VerifyOneCallBackSet(nameof(TranscribeFileCallBackAsync), callBack, prerecordedSchema);
        if (callBack != null)
            prerecordedSchema.Callback = callBack;
        var stringedOptions = QueryParameterUtil.GetParameters(prerecordedSchema);
        return await PostAsync<AsyncPrerecordedResponse>(
            $"{_urlPrefix}?{stringedOptions}",
            RequestContentUtil.CreateStreamPayload(source));
    }

    /// <summary>
    /// Transcribe a file by providing a url and a CallBack
    /// </summary>
    /// <param name="source">Url to the file that is to be transcribed <see cref="UrlSource"/></param>
    /// <param name="callBack">CallBack url</param>    
    /// <param name="prerecordedSchema">Options for the transcription<see cref="PrerecordedSchema"></param>
    /// <returns><see cref="AsyncPrerecordedResponse"/></returns>
    public async Task<AsyncPrerecordedResponse> TranscribeUrlCallBackAsync(UrlSource source, string? callBack, PrerecordedSchema? prerecordedSchema)
    {
        VerifyOneCallBackSet(nameof(TranscribeUrlCallBackAsync), callBack, prerecordedSchema);

        if (callBack != null)
            prerecordedSchema.Callback = callBack;
        var stringedOptions = QueryParameterUtil.GetParameters(prerecordedSchema);
        return await PostAsync<AsyncPrerecordedResponse>(
            $"{_urlPrefix}?{stringedOptions}",
            RequestContentUtil.CreatePayload(source));
    }
    #endregion

    #region CallbackChecks
    private static void VerifyNoCallBack(string method, PrerecordedSchema? prerecordedSchema)
    {
        if (prerecordedSchema != null && prerecordedSchema.Callback != null)
            throw new Exception($"CallBack cannot be provided as schema option to a synchronous transcription. Use {nameof(TranscribeFileCallBackAsync)} or {nameof(TranscribeUrlCallBackAsync)}");
    }

    private static void VerifyOneCallBackSet(string callingMethod, string? callBack, PrerecordedSchema? prerecordedSchema)
    {
        //check if no CallBack set in either callBack parameter or PrerecordedSchema
        if (prerecordedSchema.Callback == null && callBack == null)
            throw new Exception($"Either provide a CallBack url or set PrerecordedSchema.CallBack.  If no CallBack needed either {nameof(TranscribeUrlAsync)} or {nameof(TranscribeFileAsync)}");

        //check that only one CallBack is set in either callBack parameter or PrerecordedSchema
        if (!string.IsNullOrEmpty(prerecordedSchema.Callback) && !string.IsNullOrEmpty(callBack))
            throw new Exception("CallBack should be set in either the CallBack parameter or PrerecordedSchema.CallBack not in both.");
    }
    #endregion    
}
