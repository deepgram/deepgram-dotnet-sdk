using Deepgram.Models.Options;

namespace Deepgram.Abstractions
{
    public abstract class AbstractRestClient
    {
        internal ILogger Logger { get; set; }
        internal HttpClient HttpClient { get; set; }
        internal AbstractRestClient(string? apiKey, DeepgramClientOptions clientOptions, HttpClient httpClient, ILogger logger)
        {
            HttpClient = HttpConfigureUtil.Configure(apiKey, clientOptions, httpClient);
            Logger = logger;
        }
    }
}
