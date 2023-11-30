namespace Deepgram.Abstractions
{
    public abstract class AbstractRestClient
    {
        /// <summary>
        /// logger create by the descended class and passed in through the constructor
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        ///  HttpClient created by the factory
        internal HttpClient? HttpClient { get; set; }

        /// <summary>
        /// Options for setting HttpClient and request
        /// </summary>
        internal DeepgramClientOptions DeepgramClientOptions { get; set; }

        static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        /// <summary>
        /// Constructor that take a IHttpClientFactory
        /// </summary>
        /// <param name="apiKey">ApiKey used for Authentication Header and is required</param>
        /// <param name="loggerName">nameof the descendent class</param>
        /// <param name="httpClientFactory">IHttpClientFactory for creating instances of HttpClient for making Rest calls</param>
        internal AbstractRestClient(string? apiKey, IHttpClientFactory httpClientFactory, string loggerName, DeepgramClientOptions? deepgramClientOptions = null)
        {
            if (apiKey is null)
                throw new ArgumentException("A Deepgram API Key is required when creating a client");

            DeepgramClientOptions = deepgramClientOptions is null ? new DeepgramClientOptions() : deepgramClientOptions;
            HttpClient = HttpClientUtil.Configure(apiKey!, DeepgramClientOptions, httpClientFactory);
            Logger = LogProvider.GetLogger(loggerName);
        }


        /// <summary>
        /// GET Rest Request
        /// </summary>
        /// <typeparam name="T">Type of class of response expected</typeparam>
        /// <param name="uriSegment">request uri Endpoint</param>
        /// <returns></returns>
        public virtual async Task<T> GetAsync<T>(string uriSegment)
        {
            try
            {
                CheckForTimeout();
                var response = await HttpClient.GetAsync(uriSegment);
                response.EnsureSuccessStatusCode();
                var result = await Deserialize<T>(response);
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
        /// <typeparam name="T">Class type of what return type is expected</typeparam>
        /// <param name="uriSegment">Uri for the api including the query parameters</param> 
        /// <param name="logger">logger to log any messages</param>   
        /// <returns>instance of TResponse</returns>
        public virtual async Task<T> PostAsync<T>(string uriSegment, StringContent content)
        {
            try
            {
                CheckForTimeout();
                var response = await HttpClient.PostAsync(uriSegment, content);
                response.EnsureSuccessStatusCode();
                var result = await Deserialize<T>(response);

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred during POST request");
                throw;
            }
        }

        /// <summary>
        /// Post method
        /// </summary>
        /// <typeparam name="T">Class type of what return type is expected</typeparam>
        /// <param name="uriSegment">Uri for the api including the query parameters</param> 
        /// <param name="logger">logger to log any messages</param>   
        /// <returns>instance of TResponse</returns>
        public virtual async Task<T> PostAsync<T>(string uriSegment, HttpContent content)
        {
            try
            {
                CheckForTimeout();
                var response = await HttpClient.PostAsync(uriSegment, content);
                response.EnsureSuccessStatusCode();
                var result = await Deserialize<T>(response);

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
        public async Task Delete(string uriSegment)
        {
            try
            {
                CheckForTimeout();
                var rq = new HttpRequestMessage(HttpMethod.Delete, uriSegment);
                var response = await HttpClient.DeleteAsync(uriSegment);
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
        /// <typeparam name="T">Class Type of expected response</typeparam>
        /// <param name="uriSegment">Uri for the api including the query parameters</param>
        /// <param name="logger">logger to log any messages</param>
        /// <returns>instance  of TResponse or throws Exception</returns>
        public async Task<T> DeleteAsync<T>(string uriSegment)
        {
            try
            {
                CheckForTimeout();
                var response = await HttpClient.DeleteAsync(uriSegment);
                response.EnsureSuccessStatusCode();
                var result = await Deserialize<T>(response);

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred during DELETE request");
                throw;
            }
        }

        /// <summary>
        /// Patch method call that takes a body object
        /// </summary>
        /// <typeparam name="T">Class type of what return type is expected</typeparam>
        /// <param name="uriSegment">Uri for the api including the query parameters</param>  
        /// <returns>instance of TResponse</returns>
        public virtual async Task<T> PatchAsync<T>(string uriSegment, StringContent content)
        {
            try
            {
                CheckForTimeout();

#if NETSTANDARD2_0
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), uriSegment) { Content = content };
                var response = await HttpClient.SendAsync(request);
#else
                var response = await HttpClient.PatchAsync(uriSegment, content);
#endif
                response.EnsureSuccessStatusCode();
                var result = await Deserialize<T>(response);
                return result;

            }

            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred during PATCH request");
                throw;
            }
        }

        /// <summary>
        /// Put method call that takes a body object
        /// </summary>
        /// <typeparam name="T">Class type of what return type is expected</typeparam>
        /// <param name="uriSegment">Uri for the api</param>   
        /// <returns>instance of TResponse</returns>
        public virtual async Task<T> PutAsync<T>(string uriSegment, StringContent content)
        {
            try
            {
                CheckForTimeout();
                var response = await HttpClient.PutAsync(uriSegment, content);
                response.EnsureSuccessStatusCode();
                var result = await Deserialize<T>(response);

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred during PUT request");
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
                var deepgramResponse = JsonSerializer.Deserialize<TResponse>(content, JsonSerializerOptions);
                return deepgramResponse;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred whilst processing REST response : {ErrorMessage}", ex.Message);
                throw;
            }
        }

        internal void CheckForTimeout()
        {
            if (DeepgramClientOptions.Timeout != null)
                HttpClient.Timeout = (TimeSpan)DeepgramClientOptions.Timeout;
        }

        /// <summary>
        /// Set the time out on the HttpClient
        /// </summary>
        /// <param name="timeSpan"></param>
        public void SetTimeout(TimeSpan timeSpan)
            => DeepgramClientOptions.Timeout = timeSpan;

        /// <summary>
        /// Create the body payload of a httpRequest
        /// </summary>
        /// <typeparam name="T">Type of the body to be sent</typeparam>
        /// <param name="body">instance value for the body</param>
        /// <param name="contentType">What type of content is being sent default is : application/json</param>
        /// <returns></returns>
        internal static StringContent CreatePayload<T>(T body) => new(
                        JsonSerializer.Serialize(body, JsonSerializerOptions),
                        Encoding.UTF8,
                        Constants.DEFAULT_CONTENT_TYPE);


        /// <summary>
        /// Create the stream payload of a httpRequest
        /// </summary>
        /// <param name="body">of type stream</param>
        /// <returns>HttpContent</returns>
        internal static HttpContent CreateStreamPayload(Stream body)
        {
            body.Seek(0, SeekOrigin.Begin);
            HttpContent httpContent = new StreamContent(body);
            httpContent.Headers.Add("Content-Type", Constants.DEEPGRAM_CONTENT_TYPE);
            httpContent.Headers.Add("Content-Length", body.Length.ToString());
            return httpContent;
        }

    }
}
