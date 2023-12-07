using Deepgram.Models;

namespace Deepgram.Tests.Fakes;

public class ConcreteRestClient(DeepgramClientOptions deepgramClientOptions, HttpClient httpClient)
    : AbstractRestClient(deepgramClientOptions, httpClient)
{
}
