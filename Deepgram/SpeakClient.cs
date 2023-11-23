namespace Deepgram;
public class SpeakClient : AbstractRestClient
{
    /// <summary>
    /// Constructor that take a IHttpClientFactory
    /// </summary>
    /// <param name="apiKey">ApiKey used for Authentication Header and is required</param>
    /// <param name="clientOptions">Optional HttpClient for configuring the HttpClient</param>
    /// <param name="loggerName">nameof the descendent class</param>
    /// <param name="httpClientFactory">IHttpClientFactory for creating instances of HttpClient for making Rest calls</param>
    internal SpeakClient(string? apiKey, DeepgramClientOptions clientOptions, IHttpClientFactory httpClientFactory)
        : base(apiKey, clientOptions, nameof(SpeakClient), httpClientFactory) { }

    /// <summary>
    /// Constructor that take a IHttpClientFactory
    /// </summary>
    /// <param name="apiKey">ApiKey used for Authentication Header and is required</param>
    /// <param name="loggerName">nameof the descendent class</param>
    /// <param name="httpClientFactory">IHttpClientFactory for creating instances of HttpClient for making Rest calls</param>
    internal SpeakClient(string? apiKey, IHttpClientFactory httpClientFactory)
        : base(apiKey, nameof(SpeakClient), httpClientFactory) { }
}
