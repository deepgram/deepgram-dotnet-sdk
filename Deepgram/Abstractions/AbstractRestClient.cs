using Deepgram.Logger;


namespace Deepgram.Abstractions
{
    public abstract class AbstractRestClient
    {
        /// <summary>
        ///  HttpClient created by the factory
        internal HttpClient? _httpClient;

        /// <summary>
        /// Options for setting HttpClient and request
        /// </summary>
        internal DeepgramClientOptions _deepgramClientOptions;

        internal string _clientName;
        internal ILogger logger => LogProvider.GetLogger(_clientName);



        /// <summary>
        /// Constructor that take a IHttpClientFactory
        /// </summary>
        /// <param name="apiKey">ApiKey used for Authentication Header and is required</param>
        /// <param name="httpClientFactory"><see cref="IHttpClientFactory"/> for creating instances of HttpClient for making Rest calls</param>
        /// <param name="deepgramClientOptions"><see cref="_deepgramClientOptions"/> for HttpClient Configuration</param>
        internal AbstractRestClient(string? apiKey, IHttpClientFactory httpClientFactory, DeepgramClientOptions? deepgramClientOptions = null)
        {
            _clientName = this.GetType().Name;
            var validatedKey = ApiKeyUtil.Validate(apiKey, _clientName);
            _deepgramClientOptions = deepgramClientOptions is null ? new DeepgramClientOptions() : deepgramClientOptions;
            _httpClient = HttpClientUtil.Configure(validatedKey, _deepgramClientOptions, httpClientFactory);
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
                var response = await _httpClient.GetAsync(uriSegment);
                response.EnsureSuccessStatusCode();
                var result = await RequestContentUtil.DeserializeAsync<T>(response);
                return result;
            }
            catch (HttpRequestException hre)
            {
                Log.HttpRequestException(logger, "GET", uriSegment, hre);
                throw new HttpRequestException($"Error occurred during GET request to {uriSegment}: Message{hre.Message}", hre);
            }
            catch (Exception ex)
            {
                Log.Exception(logger, "GET", ex.GetType().Name, ex);
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
                var response = await _httpClient.PostAsync(uriSegment, content);
                response.EnsureSuccessStatusCode();
                var result = await RequestContentUtil.DeserializeAsync<T>(response);

                return result;
            }
            catch (HttpRequestException hre)
            {
                Log.HttpRequestException(logger, "POST", uriSegment, hre);
                throw new HttpRequestException($"Error occurred during GET request to {uriSegment}: Message{hre.Message}", hre);
            }
            catch (Exception ex)
            {
                Log.Exception(logger, "POST", ex.GetType().Name, ex);
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
                var response = await _httpClient.PostAsync(uriSegment, content);
                response.EnsureSuccessStatusCode();
                var result = await RequestContentUtil.DeserializeAsync<T>(response);

                return result;
            }
            catch (HttpRequestException hre)
            {
                Log.HttpRequestException(logger, "POST", uriSegment, hre);
                throw new HttpRequestException($"Error occurred during GET request to {uriSegment}: Message{hre.Message}", hre);
            }
            catch (Exception ex)
            {
                Log.Exception(logger, "POST", ex.GetType().Name, ex);
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
                var response = await _httpClient.DeleteAsync(uriSegment);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException hre)
            {
                Log.HttpRequestException(logger, "DELETE", uriSegment, hre);
                throw new HttpRequestException($"Error occurred during DELETE request to {uriSegment}: Message{hre.Message}", hre);
            }
            catch (Exception ex)
            {
                Log.Exception(logger, "DELETE", ex.GetType().Name, ex);
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
                var response = await _httpClient.DeleteAsync(uriSegment);
                response.EnsureSuccessStatusCode();
                var result = await RequestContentUtil.DeserializeAsync<T>(response);

                return result;
            }
            catch (HttpRequestException hre)
            {
                Log.HttpRequestException(logger, "DELETE ASYNC", uriSegment, hre);
                throw new HttpRequestException($"Error occurred during DELETE ASYNC request to {uriSegment}: Message{hre.Message}", hre);
            }
            catch (Exception ex)
            {
                Log.Exception(logger, "DELETE ASYNC", ex.GetType().Name, ex);
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
                var response = await _httpClient.SendAsync(request);
#else
                var response = await _httpClient.PatchAsync(uriSegment, content);
#endif
                response.EnsureSuccessStatusCode();
                var result = await RequestContentUtil.DeserializeAsync<T>(response);
                return result;

            }
            catch (HttpRequestException hre)
            {
                Log.HttpRequestException(logger, "PATCH", uriSegment, hre);
                throw new HttpRequestException($"Error occurred during PATCH request to {uriSegment}: Message{hre.Message}", hre);
            }
            catch (Exception ex)
            {
                Log.Exception(logger, "PATCH", ex.GetType().Name, ex);
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
                var response = await _httpClient.PutAsync(uriSegment, content);
                response.EnsureSuccessStatusCode();
                var result = await RequestContentUtil.DeserializeAsync<T>(response);

                return result;
            }
            catch (HttpRequestException hre)
            {
                Log.HttpRequestException(logger, "PUT", uriSegment, hre);
                throw new HttpRequestException($"Error occurred during PUT request to {uriSegment}: Message{hre.Message}", hre);
            }
            catch (Exception ex)
            {
                Log.Exception(logger, "PUT", ex.GetType().Name, ex);
                throw new Exception($"Error occurred during PUT request to {uriSegment}: Message {ex.Message} ", ex);
            }
        }



        internal void CheckForTimeout()
        {
            if (_deepgramClientOptions.Timeout != null)
                _httpClient.Timeout = (TimeSpan)_deepgramClientOptions.Timeout;
        }

        /// <summary>
        /// Set the time out on the HttpClient
        /// </summary>
        /// <param name="timeSpan"></param>
        public void SetTimeout(TimeSpan timeSpan)
            => _deepgramClientOptions.Timeout = timeSpan;

    }
}
