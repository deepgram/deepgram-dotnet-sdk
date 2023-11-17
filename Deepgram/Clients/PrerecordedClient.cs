
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

    //https://github.com/deepgram/deepgram-node-sdk/blob/lo/beta-test-improvements/src/lib/helpers.ts
    //https://github.com/deepgram/deepgram-node-sdk/blob/lo/beta-test-improvements/src/lib/types/PrerecordedSource.ts
    //can take a UrlSource
    //byte[] (c# equivalent to node buffer)
    //Stream (c# equivalent to node readable)
    public async Task<SyncPrerecordedResponse> TranscribeUrl(UrlSource source, PrerecordedSchema? prerecordedSchema)
    {
        var stringedOptions = QueryParameterUtil.GetParameters(prerecordedSchema);
        string url = $"listen?{stringedOptions}";
        throw new NotImplementedException();
    }

    //https://github.com/deepgram/deepgram-node-sdk/blob/lo/beta-test-improvements/src/lib/helpers.ts
    //https://github.com/deepgram/deepgram-node-sdk/blob/lo/beta-test-improvements/src/lib/types/PrerecordedSource.ts
    //byte[] (c# equivalent to node buffer)
    //Stream (c# equivalent to node readable)
    public async Task<SyncPrerecordedResponse> TranscribeFile(FileSource fileSource, PrerecordedSchema? prerecordedSchema)
    {
        var stringedOptions = QueryParameterUtil.GetParameters(prerecordedSchema);
        string url = $"listen?{stringedOptions}";
        throw new NotImplementedException();
    }

    //https://github.com/deepgram/deepgram-node-sdk/blob/lo/beta-test-improvements/src/lib/helpers.ts
    //https://github.com/deepgram/deepgram-node-sdk/blob/lo/beta-test-improvements/src/lib/types/PrerecordedSource.ts
    //can take a UrlSource
    //byte[] (c# equivalent to node buffer)
    //Stream (c# equivalent to node readable)
    public async Task<AsyncPrerecordedResponse> TranscribeUrlCallback(UrlSource source, string callBack, PrerecordedSchema? prerecordedSchema)
    {
        var stringedOptions = QueryParameterUtil.GetParameters(prerecordedSchema);
        string url = $"listen?{stringedOptions}";
        throw new NotImplementedException();
    }

    //https://github.com/deepgram/deepgram-node-sdk/blob/lo/beta-test-improvements/src/lib/helpers.ts
    //https://github.com/deepgram/deepgram-node-sdk/blob/lo/beta-test-improvements/src/lib/types/PrerecordedSource.ts
    //byte[] (c# equivalent to node buffer)
    //Stream (c# equivalent to node readable)
    public async Task<AsyncPrerecordedResponse> TranscribeFileCallback(FileSource fileSource, string callBack, PrerecordedSchema? prerecordedSchema)
    {
        var stringedOptions = QueryParameterUtil.GetParameters(prerecordedSchema);
        string url = $"listen?{stringedOptions}";
        throw new NotImplementedException();
    }


}
