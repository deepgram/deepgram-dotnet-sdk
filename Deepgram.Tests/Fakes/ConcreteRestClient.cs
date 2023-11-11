using Deepgram.Abstractions;
using Deepgram.Models.Options;

namespace Deepgram.Tests.Fakes;

public class ConcreteRestClient : AbstractRestClient
{
    public ConcreteRestClient(string? apiKey, DeepgramClientOptions clientOptions, HttpClient httpClient)
        : base(apiKey, clientOptions, httpClient, LogProvider.GetLogger(nameof(ConcreteRestClient)))
    {
    }
}
