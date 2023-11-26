namespace Deepgram;
public class ReadClient : AbstractRestClient
{
    /// <summary>
    /// Constructor that take a IHttpClientFactory
    /// </summary>
    /// <param name="apiKey">ApiKey used for Authentication Header and is required</param>
    /// <param name="loggerName">nameof the descendent class</param>
    /// <param name="httpClientFactory">IHttpClientFactory for creating instances of HttpClient for making Rest calls</param>
    internal ReadClient(string? apiKey, IHttpClientFactory httpClientFactory)
        : base(apiKey, httpClientFactory, nameof(ReadClient)) { }
}
