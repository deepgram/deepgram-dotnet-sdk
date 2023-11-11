using Deepgram.Models.Options;

namespace Deepgram.Abstractions
{
    public abstract class AbstractRestClient
    {
        internal ILogger Logger { get; set; }
        internal HttpClient HttpClient { get; set; }
        internal DeepgramClientOptions? Options { get; }
        internal string? ApiKey { get; }

        internal AbstractRestClient(string? apiKey, DeepgramClientOptions clientOptions, HttpClient httpClient, ILogger logger)
        {

            //  HttpClient = HttpConfigureUtil.SetBaseUrl(clientOptions.Url, httpClient);
            Options = clientOptions;
            Logger = logger;
            ApiKey = apiKey;
            //HttpClient = HttpConfigureUtil.SetHeaders(ApiKey, null, httpClient);
            HttpClient = httpClient;
        }
    }
}
