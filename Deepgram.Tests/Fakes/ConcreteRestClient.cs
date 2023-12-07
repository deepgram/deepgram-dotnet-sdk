using Deepgram.Models;

namespace Deepgram.Tests.Fakes;

public class ConcreteRestClient(DeepgramClientOptions deepgramClientOptions, IHttpClientFactory httpClientFactory)
    : AbstractRestClient(deepgramClientOptions, httpClientFactory)
{
}
