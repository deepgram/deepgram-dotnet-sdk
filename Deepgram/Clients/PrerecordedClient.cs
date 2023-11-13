using Deepgram.Models.Options;

namespace Deepgram.Clients;
public class PrerecordedClient : AbstractRestClient
{
    public PrerecordedClient(string? apiKey, DeepgramClientOptions clientOptions, HttpClient httpClient)
        : base(apiKey, clientOptions, httpClient, LogProvider.GetLogger(nameof(PrerecordedClient)))
    {
    }
}
