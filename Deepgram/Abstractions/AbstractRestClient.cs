using Deepgram.Logger;

namespace Deepgram.Abstractions
{
    public abstract class AbstractRestClient
    {
        /// <summary>
        ///  HttpClient created by the factory
        internal HttpClient? HttpClient { get; set; }

        public ILogger Logger { get; set; }

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
        /// <param name="httpClientFactory"><see cref="IHttpClientFactory"/> for creating instances of HttpClient for making Rest calls</param>
        /// <param name="deepgramClientOptions"><see cref="DeepgramClientOptions"/> for HttpClient Configuration</param>
        internal AbstractRestClient(string? apiKey, IHttpClientFactory httpClientFactory, string loggerName, DeepgramClientOptions? deepgramClientOptions = null)
        {
            Logger = LogProvider.GetLogger(loggerName);

            if (string.IsNullOrEmpty(apiKey))
            {
                Log.ApiKeyNotPresent(Logger, this.GetType().Name);
                throw new ArgumentException("A Deepgram API Key is required when creating a client");
            }
            DeepgramClientOptions = deepgramClientOptions is null ? new DeepgramClientOptions() : deepgramClientOptions;
            HttpClient = HttpClientUtil.Configure(apiKey!, DeepgramClientOptions, httpClientFactory);

        }


        /// <summary>
        /// GET Rest Request
        /// </summary>
        /// <typeparam name="T">Type of class of response expected</typeparam>
        /// <param name="uriSegment">request uri Endpoint</param>
        /// <returns>Instance of T</returns>
        public async Task<T> GetAsync<T>(string uriSegment)
        {
            try
            {
                CheckForTimeout();
                var response = await HttpClient.GetAsync(uriSegment);
                response.EnsureSuccessStatusCode();
                var result = await Deserialize<T>(Logger, response);
                return result;
            }
            catch (HttpRequestException hre)
            {
                Log.HttpRequestException(Logger, "GET", uriSegment, hre);
                throw new HttpRequestException($"Error occurred during GET request to {uriSegment}: Message{hre.Message}", hre);
            }
            catch (Exception ex)
            {
                Log.Exception(Logger, "GET", ex.GetType().Name, ex);
                throw new Exception($"Error occurred during GET request to {uriSegment}: Message{ex.Message}", ex);
            }
        }

        /// <summary>
        /// Post method
        /// </summary>
        /// <typeparam name="T">Class type of what return type is expected</typeparam>
        /// <param name="uriSegment">Uri for the api including the query parameters</param>   
        /// <param name="content">StringContent as content for HttpRequestMessage</param>   
        /// <returns>Instance of T</returns>
        public async Task<T> PostAsync<T>(string uriSegment, StringContent content)
        {
            try
            {
                CheckForTimeout();
                var response = await HttpClient.PostAsync(uriSegment, content);
                response.EnsureSuccessStatusCode();
                var result = await Deserialize<T>(Logger, response);

                return result;
            }
            catch (HttpRequestException hre)
            {
                Log.HttpRequestException(Logger, "POST", uriSegment, hre);
                throw new HttpRequestException($"Error occurred during GET request to {uriSegment}: Message{hre.Message}", hre);
            }
            catch (Exception ex)
            {
                Log.Exception(Logger, "POST", ex.GetType().Name, ex);
                throw new Exception($"Error occurred during StringContent POST request to {uriSegment}: Message {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Post method for use with stream requests
        /// </summary>
        /// <typeparam name="T">Class type of what return type is expected</typeparam>
        /// <param name="uriSegment">Uri for the api including the query parameters</param> 
        /// <param name="content">HttpContent as content for HttpRequestMessage</param>  
        /// <returns>Instance of T</returns>
        public async Task<T> PostAsync<T>(string uriSegment, HttpContent content)
        {
            try
            {
                CheckForTimeout();
                var response = await HttpClient.PostAsync(uriSegment, content);
                response.EnsureSuccessStatusCode();
                var result = await Deserialize<T>(Logger, response);

                return result;
            }
            catch (HttpRequestException hre)
            {
                Log.HttpRequestException(Logger, "POST", uriSegment, hre);
                throw new HttpRequestException($"Error occurred during GET request to {uriSegment}: Message{hre.Message}", hre);
            }
            catch (Exception ex)
            {
                Log.Exception(Logger, "POST", ex.GetType().Name, ex);
                throw new Exception($"Error occurred during HTTPContent POST request to {uriSegment}", ex);
            }

        }


        /// <summary>
        /// Delete Method for use with calls that do not expect a response
        /// </summary>
        /// <param name="uriSegment">Uri for the api including the query parameters</param> 
        public async Task Delete(string uriSegment)
        {
            try
            {
                CheckForTimeout();
                var rq = new HttpRequestMessage(HttpMethod.Delete, uriSegment);
                var response = await HttpClient.DeleteAsync(uriSegment);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException hre)
            {
                Log.HttpRequestException(Logger, "DELETE", uriSegment, hre);
                throw new HttpRequestException($"Error occurred during DELETE request to {uriSegment}: Message{hre.Message}", hre);
            }
            catch (Exception ex)
            {
                Log.Exception(Logger, "DELETE", ex.GetType().Name, ex);
                throw new Exception($"Error occurred during DELETE request to {uriSegment}: Message {ex.Message} ", ex);
            }
        }

        /// <summary>
        /// Delete method that returns the type of response specified
        /// </summary>
        /// <typeparam name="T">Class Type of expected response</typeparam>
        /// <param name="uriSegment">Uri for the api including the query parameters</param>      
        /// <returns>Instance  of T or throws Exception</returns>
        public async Task<T> DeleteAsync<T>(string uriSegment)
        {
            try
            {
                CheckForTimeout();
                var response = await HttpClient.DeleteAsync(uriSegment);
                response.EnsureSuccessStatusCode();
                var result = await Deserialize<T>(Logger, response);

                return result;
            }
            catch (HttpRequestException hre)
            {
                Log.HttpRequestException(Logger, "DELETE ASYNC", uriSegment, hre);
                throw new HttpRequestException($"Error occurred during DELETE ASYNC request to {uriSegment}: Message{hre.Message}", hre);
            }
            catch (Exception ex)
            {
                Log.Exception(Logger, "DELETE ASYNC", ex.GetType().Name, ex);
                throw new Exception($"Error occurred during DELETE ASYNC request to {uriSegment}: Message {ex.Message} ", ex);
            }
        }

        /// <summary>
        /// Patch method call that takes a body object
        /// </summary>
        /// <typeparam name="T">Class type of what return type is expected</typeparam>
        /// <param name="uriSegment">Uri for the api including the query parameters</param>  
        /// <returns>Instance of T</returns>
        public async Task<T> PatchAsync<T>(string uriSegment, StringContent content)
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
                var result = await Deserialize<T>(Logger, response);
                return result;

            }
            catch (HttpRequestException hre)
            {
                Log.HttpRequestException(Logger, "PATCH", uriSegment, hre);
                throw new HttpRequestException($"Error occurred during PATCH request to {uriSegment}: Message{hre.Message}", hre);
            }
            catch (Exception ex)
            {
                Log.Exception(Logger, "PATCH", ex.GetType().Name, ex);
                throw new Exception($"Error occurred during PATCH request to {uriSegment}: Message {ex.Message} ", ex);
            }

        }

        /// <summary>
        /// Put method call that takes a body object
        /// </summary>
        /// <typeparam name="T">Class type of what return type is expected</typeparam>
        /// <param name="uriSegment">Uri for the api</param>   
        /// <returns>Instance of T</returns>
        public async Task<T> PutAsync<T>(string uriSegment, StringContent content)
        {
            try
            {
                CheckForTimeout();
                var response = await HttpClient.PutAsync(uriSegment, content);
                response.EnsureSuccessStatusCode();
                var result = await Deserialize<T>(Logger, response);

                return result;
            }
            catch (HttpRequestException hre)
            {
                Log.HttpRequestException(Logger, "PUT", uriSegment, hre);
                throw new HttpRequestException($"Error occurred during PUT request to {uriSegment}: Message{hre.Message}", hre);
            }
            catch (Exception ex)
            {
                Log.Exception(Logger, "PUT", ex.GetType().Name, ex);
                throw new Exception($"Error occurred during PUT request to {uriSegment}: Message {ex.Message} ", ex);
            }
        }


        /// <summary>
        /// method that deserializes DeepgramResponse and performs null checks on values
        /// </summary>
        /// <typeparam name="TResponse">Class Type of expected response</typeparam>
        /// <param name="httpResponseMessage">Http Response to be deserialized</param>       
        /// <returns>instance of TResponse or a Exception</returns>
        internal static async Task<TResponse> Deserialize<TResponse>(ILogger logger, HttpResponseMessage httpResponseMessage)
        {
            try
            {
                var content = await httpResponseMessage.Content.ReadAsStringAsync();
                var deepgramResponse = JsonSerializer.Deserialize<TResponse>(content, JsonSerializerOptions);
                return deepgramResponse;
            }
            catch (Exception ex)
            {
                Log.SerializerException(logger, "Deserializing", ex.GetType().Name, typeof(TResponse).Name, ex);
                throw new Exception($"Error occurred whilst processing REST response : {ex.Message}", ex);
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
        /// Create the body payload of a HttpRequestMessage
        /// </summary>
        /// <typeparam name="T">Type of the body to be sent</typeparam>
        /// <param name="body">instance value for the body</param>
        /// <param name="contentType">What type of content is being sent default is : application/json</param>
        /// <returns></returns>
        internal static StringContent CreatePayload<T>(ILogger logger, T body)
        {
            try
            {
                return new StringContent(
                                    JsonSerializer.Serialize(body, JsonSerializerOptions),
                                   Encoding.UTF8,
                                   Constants.DEFAULT_CONTENT_TYPE);
            }
            catch (Exception ex)
            {
                Log.SerializerException(logger, "Serializing", ex.GetType().Name, typeof(T).Name, ex);
                throw new Exception($"Error occurred whilst creating http request message body using data of type {typeof(T).Name}: ErrorMessage: {ex.Message}", ex);
            }

        }


        /// <summary>
        /// Create the stream payload of a HttpRequestMessage
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
