using System.Text;
using System.Text.Json;
using Deepgram.Models.Options;

namespace Deepgram.Abstractions
{
    public abstract class AbstractRestClient
    {
        /// <summary>
        /// logger create by the descended class and passed in through the constructor
        /// </summary>
        internal ILogger Logger { get; set; }

        /// <summary>
        /// Optional IHttpClientFactory passed in by the consuming project
        /// </summary>
        internal IHttpClientFactory? HttpClientFactory { get; set; }

        /// <summary>
        /// Optional HttpClient passed in by consuming project
        /// </summary>
        internal HttpClient? ExternalHttpClient { get; set; }

        /// <summary>
        /// ApiKey used for Authentication Header and is required
        /// </summary>
        internal string ApiKey { get; set; }

        /// <summary>
        /// Timeout for the HttpClient
        /// </summary>
        internal TimeSpan? Timeout { get; set; }

        /// <summary>
        /// Options for configuring the HttpClient
        /// </summary>
        internal DeepgramClientOptions Options { get; set; }

        /// <summary>
        /// Constructor that take a HttpClient
        /// </summary>
        /// <param name="apiKey">ApiKey used for Authentication Header and is required</param>
        /// <param name="clientOptions">Optional HttpClient for configuring the HttpClient</param>
        /// <param name="loggerName">nameof the descendent class</param>
        /// <param name="httpClient">HttpClient for making Rest calls</param>
        internal AbstractRestClient(string? apiKey, DeepgramClientOptions clientOptions, string loggerName, HttpClient httpClient)
        {
            ApiKey = ApiKeyUtil.Configure(apiKey);
            ExternalHttpClient = httpClient;
            Options = clientOptions;
            Logger = LogProvider.GetLogger(loggerName);
        }

        /// <summary>
        /// Constructor that take a IHttpClientFactory
        /// </summary>
        /// <param name="apiKey">ApiKey used for Authentication Header and is required</param>
        /// <param name="clientOptions">Optional HttpClient for configuring the HttpClient</param>
        /// <param name="loggerName">nameof the descendent class</param>
        /// <param name="httpClientFactory">IHttpClientFactory for creating instances of HttpClient for making Rest calls</param>
        internal AbstractRestClient(string? apiKey, DeepgramClientOptions clientOptions, string loggerName, IHttpClientFactory httpClientFactory)
        {
            ApiKey = ApiKeyUtil.Configure(apiKey);
            HttpClientFactory = httpClientFactory;
            Options = clientOptions;
            Logger = LogProvider.GetLogger(loggerName);
        }



        /// <summary>
        /// Set up the HttpClient onb method call using in priority order: IHttpClientFactory,ExternalHttpClient, Fallback - create a new HttpClient
        /// </summary>
        /// <returns>A HttpClient, with timeout,headers a</returns>
        internal HttpClient ConfigureClient()
        {
            HttpClient client;
            if (HttpClientFactory != null)
                client = HttpClientFactory.CreateClient();
            else if (ExternalHttpClient != null)
                client = ExternalHttpClient;
            else
                client = new HttpClient();

            if (Timeout != null)
                client.Timeout = (TimeSpan)Timeout;

            return HttpConfigureUtil.Configure(ApiKey, Options, client);
        }

        /// <summary>
        /// Set the time out on the httpclient
        /// </summary>
        /// <param name="timeSpan"></param>
        public void SetTimeout(TimeSpan timeSpan)
        {
            Timeout = timeSpan;
        }

        /// <summary>
        /// Create the body payload of a httpRequest
        /// </summary>
        /// <typeparam name="T">Type of the body to be sent</typeparam>
        /// <param name="body">instance valye for the body</param>
        /// <param name="contentType">What type of content is being sent default is : application/json</param>
        /// <returns></returns>
        internal static StringContent CreatePayload<T>(T body, string contentType = Constants.DEFAULT_CONTENT_TYPE)
        {
            return new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                contentType);
        }
    }
}
