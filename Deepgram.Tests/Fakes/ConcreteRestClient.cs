namespace Deepgram.Tests.Fakes;

public class ConcreteRestClient : AbstractRestClient
{
    public ConcreteRestClient(string? apiKey, IHttpClientFactory httpClientFactory)
        : base(apiKey, httpClientFactory, nameof(ConcreteRestClient))
    {
    }
}
