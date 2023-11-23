namespace Deepgram.Tests.Fakes;

public class ConcreteRestClient : AbstractRestClient
{

    public ConcreteRestClient(string? apiKey, IHttpClientFactory httpClientFactory, DeepgramClientOptions clientOptions)
        : base(apiKey, httpClientFactory, clientOptions, nameof(ConcreteRestClient))
    {
    }
    public ConcreteRestClient(string? apiKey, IHttpClientFactory httpClientFactory)
        : base(apiKey, httpClientFactory, nameof(ConcreteRestClient))
    {
    }
}
