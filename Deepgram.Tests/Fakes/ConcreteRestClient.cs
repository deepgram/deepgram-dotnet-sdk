using Deepgram.Models.Authenticate.v1;

namespace Deepgram.Tests.Fakes;

public class ConcreteRestClient(string apiKey, DeepgramClientOptions? deepgramClientOptions)
    : AbstractRestClient(apiKey, deepgramClientOptions)
{
}
