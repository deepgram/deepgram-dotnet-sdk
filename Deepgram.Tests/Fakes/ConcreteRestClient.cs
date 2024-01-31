using Deepgram.Models.Shared.v1;

namespace Deepgram.Tests.Fakes;

public class ConcreteRestClient(string apiKey, DeepgramClientOptions? deepgramClientOptions)
    : AbstractRestClient(apiKey, deepgramClientOptions)
{
}
