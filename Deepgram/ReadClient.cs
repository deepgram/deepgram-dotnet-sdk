namespace Deepgram;

/// <summary>
/// Constructor that take a IHttpClientFactory
/// </summary>
/// <param name="apiKey">ApiKey used for Authentication Header and is required</param>
/// <param name="httpClientFactory"><see cref="IHttpClientFactory"/> for creating instances of HttpClient for making Rest calls</param>
/// <param name="deepgramClientOptions"><see cref="DeepgramClientOptions"/> for HttpClient Configuration</param>
internal class ReadClient(string? apiKey, IHttpClientFactory httpClientFactory, DeepgramClientOptions? deepgramClientOptions = null)
    : AbstractRestClient(apiKey, httpClientFactory, deepgramClientOptions)
{

}
