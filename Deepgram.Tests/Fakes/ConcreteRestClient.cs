namespace Deepgram.Tests.Fakes;

public class ConcreteRestClient : AbstractRestClient
{
    public ConcreteRestClient(string? apiKey, IHttpClientFactory httpClientFactory, DeepgramClientOptions? deepgramClientOptions = null)
        : base(apiKey, httpClientFactory, nameof(ConcreteRestClient), deepgramClientOptions)
    {
    }
}
