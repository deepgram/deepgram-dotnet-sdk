using Deepgram.Models.Options;

namespace Deepgram.Clients;
public class PrerecordedClient : AbstractRestClient
{

    /// <summary>
    /// Constructor that take a HttpClient
    /// </summary>
    /// <param name="apiKey">ApiKey used for Authentication Header and is required</param>
    /// <param name="clientOptions">Optional HttpClient for configuring the HttpClient</param>   
    /// <param name="httpClient">HttpClient for making Rest calls</param>
    public PrerecordedClient(string? apiKey, DeepgramClientOptions clientOptions, HttpClient httpClient)
        : base(apiKey, clientOptions, nameof(PrerecordedClient), httpClient)
    {
    }

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
}
