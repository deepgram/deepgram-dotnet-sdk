namespace Deepgram.Clients;


//working of node sdk - https://github.com/deepgram/deepgram-node-sdk/blob/lo/beta-test-improvements/src/packages/PrerecordedClient.ts
public class PrerecordedClient : AbstractRestClient
{

    /// <summary>
    /// Constructor that take a IHttpClientFactory
    /// </summary>
    /// <param name="apiKey">ApiKey used for Authentication Header and is required</param>
    /// <param name="clientOptions">Optional HttpClient for configuring the HttpClient</param>   
    /// <param name="httpClientFactory">IHttpClientFactory for creating instances of HttpClient for making Rest calls</param>
    public PrerecordedClient(string? apiKey, DeepgramClientOptions clientOptions, IHttpClientFactory httpClientFactory)
        : base(apiKey, clientOptions, nameof(PrerecordedClient), httpClientFactory)
    {
    }

    /// <summary>
    ///  Transcribe a file by providing a url 
    /// </summary>
    /// <param name="source">Url to the file that is to be transcribed</param>
    /// <param name="prerecordedSchema">Option for the transcription</param>
    /// <returns>SyncPrerecordedResponse</returns>
    public async Task<SyncPrerecordedResponse> TranscribeUrl(UrlSource source, PrerecordedSchema? prerecordedSchema)
    {
        VerifyNoCallBack(prerecordedSchema);

        var stringedOptions = QueryParameterUtil.GetParameters(prerecordedSchema);
        string url = $"listen?{stringedOptions}";
        var payload = CreatePayload(source);
        return await PostAsync<SyncPrerecordedResponse>(url, payload);
    }

    /// <summary>
    /// Transcribes a file using the provided byte array
    /// </summary>
    /// <param name="source">file is the form of a byte[]</param>
    /// <param name="prerecordedSchema">Option for the transcription</param>
    /// <returns>SyncPrerecordedResponse</returns>
    public async Task<SyncPrerecordedResponse> TranscribeFile(byte[] source, PrerecordedSchema? prerecordedSchema)
    {
        VerifyNoCallBack(prerecordedSchema);
        var stringedOptions = QueryParameterUtil.GetParameters(prerecordedSchema);
        string url = $"listen?{stringedOptions}";
        var stream = new MemoryStream();
        stream.Write(source, 0, source.Length);
        var payload = CreateStreamPayload(stream);
        return await PostAsync<SyncPrerecordedResponse>(url, payload);
    }

    /// <summary>
    /// Transcribes a file using the provided stream
    /// </summary>
    /// <param name="source">file is the form of a stream</param>
    /// <param name="prerecordedSchema">Options for the transcription</param>
    /// <returns>SyncPrerecordedResponse</returns>
    public async Task<SyncPrerecordedResponse> TranscribeFile(Stream source, PrerecordedSchema? prerecordedSchema)
    {
        VerifyNoCallBack(prerecordedSchema);
        var stringedOptions = QueryParameterUtil.GetParameters(prerecordedSchema);
        string url = $"listen?{stringedOptions}";
        var payload = CreateStreamPayload(source);
        return await PostAsync<SyncPrerecordedResponse>(url, payload);
    }


    /// <summary>
    /// Transcribe a file by providing a url and a callback
    /// </summary>
    /// <param name="source">Url to the file that is to be transcribed</param>
    /// <param name="callBack">CallBack url</param>    
    /// <param name="prerecordedSchema">Options for the transcription</param>
    /// <returns>AsyncPrerecordedResponse</returns>
    public async Task<AsyncPrerecordedResponse> TranscribeUrlCallBack(UrlSource source, string? callBack, PrerecordedSchema? prerecordedSchema)
    {
        VerifyOneCallBackSet(callBack, prerecordedSchema);
        var stringedOptions = QueryParameterUtil.GetParameters(prerecordedSchema);
        string url = $"listen?{stringedOptions}";
        var payload = CreatePayload(source);
        return await PostAsync<AsyncPrerecordedResponse>(url, payload);
    }


    /// <summary>
    /// Transcribes a file using the provided byte array and providing a CallBack
    /// </summary>
    /// <param name="source">file is the form of a byte[]</param>
    /// <param name="callBack">CallBack url</param>    
    /// <param name="prerecordedSchema">Options for the transcription</param>
    /// <returns>AsyncPrerecordedResponse</returns>
    public async Task<AsyncPrerecordedResponse> TranscribeFileCallBack(byte[] source, string? callBack, PrerecordedSchema? prerecordedSchema)
    {
        var stringedOptions = QueryParameterUtil.GetParameters(prerecordedSchema);
        string url = $"listen?{stringedOptions}";
        var stream = new MemoryStream();
        stream.Write(source, 0, source.Length);
        var payload = CreateStreamPayload(stream);
        return await PostAsync<AsyncPrerecordedResponse>(url, payload);
    }

    /// <summary>
    /// Transcribes a file using the provided stream and providing a CallBack
    /// </summary>
    /// <param name="source">file is the form of a stream</param>
    /// <param name="callBack">CallBack url</param>    
    /// <param name="prerecordedSchema">Options for the transcription</param>
    /// <returns>AsyncPrerecordedResponse</returns>
    public async Task<AsyncPrerecordedResponse> TranscribeFileCallback(Stream source, string? callBack, PrerecordedSchema? prerecordedSchema)
    {
        var stringedOptions = QueryParameterUtil.GetParameters(prerecordedSchema);
        string url = $"listen?{stringedOptions}";
        var payload = CreateStreamPayload(source);
        return await PostAsync<AsyncPrerecordedResponse>(url, payload);
    }

    private static void VerifyNoCallBack(PrerecordedSchema? prerecordedSchema)
    {
        if (prerecordedSchema != null && prerecordedSchema.Callback != null)
        {
            throw new DeepgramError($"CallBack cannot be provided as schema option to a synchronous transcription. Use {nameof(TranscribeFileCallback)} or {nameof(TranscribeUrlCallBack)}");
        }
    }

    private static void VerifyOneCallBackSet(string? callBack, PrerecordedSchema? prerecordedSchema)
    {
        //check if no CallBack set in either callBack parameter or PrerecordedSchema
        if (prerecordedSchema.Callback == null && callBack == null)
        {
            throw new DeepgramError($"Either provide a CallBack url or set PrerecordedSchema.CallBack.  Use {nameof(TranscribeFile)} or {nameof(TranscribeUrl)}");
        }

        //check that only one CallBack is set in either callBack parameter or PrerecordedSchema
        if (!string.IsNullOrEmpty(prerecordedSchema.Callback) && !string.IsNullOrEmpty(callBack))
        {
            throw new DeepgramError("CallBack should be set in either the CallBack parameter or PrerecordedSchema.CallBack not in both.");
        }
    }

}
