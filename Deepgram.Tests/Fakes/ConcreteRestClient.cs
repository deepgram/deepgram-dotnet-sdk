namespace Deepgram.Tests.Fakes;

public class ConcreteRestClient(string? apiKey, IHttpClientFactory httpClientFactory, DeepgramClientOptions? deepgramClientOptions = null)
    : AbstractRestClient(apiKey, httpClientFactory, deepgramClientOptions)
{
}
