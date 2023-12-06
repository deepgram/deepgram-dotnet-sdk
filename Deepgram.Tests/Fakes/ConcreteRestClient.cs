namespace Deepgram.Tests.Fakes;

public class ConcreteRestClient(IHttpClientFactory httpClientFactory, DeepgramClientOptions deepgramClientOptions)
    : AbstractRestClient(httpClientFactory, deepgramClientOptions)
{
}
