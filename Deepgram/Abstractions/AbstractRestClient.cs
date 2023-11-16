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

        public virtual async Task<TResponse> GetAsync<TResponse>(string uriSegment)
        {
            try
            {
                var client = ConfigureClient();
                var requestUri = $"{Constants.API_VERSION}/{uriSegment}";
                var response = await client.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();
                var result = await Deserialize<TResponse>(response);
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred during GET request");
                throw;
            }
        }

        /// <summary>
        /// Post method
        /// </summary>
        /// <typeparam name="TResponse">Class type of what return type is expected</typeparam>
        /// <typeparam name="TContent">type of object being sent as part of the body</typeparam>
        /// <param name="uriSegment">Uri for the api including the query parameters</param>    
        /// <param name="obj">instance of T that is to be sent</param>
        /// <param name="logger">logger to log any messages</param>   
        /// <returns>instance of TResponse</returns>
        public virtual async Task<TResponse> PostAsync<TResponse, TContent>(string uriSegment, TContent obj)
        {
            try
            {
                var client = ConfigureClient();
                var requestUrl = $"{Constants.API_VERSION}/{uriSegment}";
                var payload = CreatePayload(obj);
                var response = await client.PostAsync(requestUrl, payload);
                response.EnsureSuccessStatusCode();
                var result = await Deserialize<TResponse>(response);

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred during POST request");
                throw;
            }
        }

        /// <summary>
        /// Delete Method for use with calls that do not expect a response
        /// </summary>
        /// <param name="uriSegment">Uri for the api including the query parameters</param>
        /// <param name="logger">logger to log any messages</param>
        /// <returns>nothing</returns>
        public async Task DeleteAsync(string uriSegment)
        {
            try
            {
                var client = ConfigureClient();
                var requestUrl = $"{Constants.API_VERSION}/{uriSegment}";

                var response = await client.DeleteAsync(requestUrl);
                response.EnsureSuccessStatusCode();

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred during DELETE request");
                throw;
            }
        }

        /// <summary>
        /// Delete method that returns the type of response specified
        /// </summary>
        /// <typeparam name="TResponse">Class Type of expected response</typeparam>
        /// <param name="uriSegment">Uri for the api including the query parameters</param>
        /// <param name="logger">logger to log any messages</param>
        /// <returns>instance  of TResponse or throws Exception</returns>
        public async Task<TResponse> DeleteAsync<TResponse>(string uriSegment)
        {
            try
            {
                var client = ConfigureClient();
                var requestUrl = $"{Constants.API_VERSION}/{uriSegment}";
                var response = await client.DeleteAsync(requestUrl);

                response.EnsureSuccessStatusCode();
                var result = await Deserialize<TResponse>(response);

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred during DELETE request");
                throw;
            }
        }



        /// <summary>
        /// method that deserializes DeepgramResponse and performs null checks on values
        /// </summary>
        /// <typeparam name="TResponse">Class Type of expected response</typeparam>
        /// <param name="httpResponseMessage">Http Response to be deserialized</param>
        /// <param name="logger">logger to log messages</param>
        /// <returns>instance of TResponse or a Exception</returns>
        internal async Task<TResponse> Deserialize<TResponse>(HttpResponseMessage httpResponseMessage)
        {
            var content = await httpResponseMessage.Content.ReadAsStringAsync();
            try
            {

                var options = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                };

                var deepgramResponse = JsonSerializer.Deserialize<TResponse>(content, options);
                return deepgramResponse;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred whilst processing REST response : {0}", ex.Message);
                throw;
            }
        }



        /// <summary>
        /// Set up the HttpClient onb method call using in priority order: IHttpClientFactory,ExternalHttpClient, Fallback - create a new HttpClient
        /// </summary>
        /// <returns>A HttpClient, with timeout,headers a</returns>
        internal HttpClient ConfigureClient()
        {
            HttpClient? client = null;
            if (HttpClientFactory != null)
                client = HttpClientFactory.CreateClient();
            else if (ExternalHttpClient != null)
                client = ExternalHttpClient;

            if (Timeout != null)
                client.Timeout = (TimeSpan)Timeout;

            return HttpConfigureUtil.Configure(ApiKey, Options, client);
        }

        /// <summary>
        /// Set the time out on the httpclient
        /// </summary>
        /// <param name="timeSpan"></param>
        public void SetTimeout(TimeSpan timeSpan) => Timeout = timeSpan;

        /// <summary>
        /// Create the body payload of a httpRequest
        /// </summary>
        /// <typeparam name="T">Type of the body to be sent</typeparam>
        /// <param name="body">instance valye for the body</param>
        /// <param name="contentType">What type of content is being sent default is : application/json</param>
        /// <returns></returns>
        internal static StringContent CreatePayload<T>(T body, string contentType = Constants.DEFAULT_CONTENT_TYPE)
        {
            var serializeOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };

            var serializedBody = JsonSerializer.Serialize(body, serializeOptions);
            return new StringContent(
                        serializedBody,
                        Encoding.UTF8,
                        contentType);
        }
    }
}
