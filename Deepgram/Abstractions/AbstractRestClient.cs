using Deepgram.Extensions;

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

        /// <summary>
        /// name of the type of client - for logging
        /// </summary>
        internal string _clientName;
        /// <summary>
        /// get the logger of catergory type of _clientName
        /// </summary>
        internal ILogger _logger => LogProvider.GetLogger(_clientName);

        /// <summary>
        /// Timeout for HttpClient
        /// </summary>
        internal TimeSpan? _timeout;

        /// <summary>
        /// Constructor that take the options and a httpClient
        /// </summary>
        /// <param name="deepgramClientOptions"><see cref="_deepgramClientOptions"/>Options for the Deepgram client</param>
        /// <param name="httpClient"><see cref="HttpClient"/>HttpClient to use for all calls from the implementing class</param>
        internal AbstractRestClient(DeepgramClientOptions deepgramClientOptions, HttpClient httpClient)
        {
            _clientName = this.GetType().Name;
            _deepgramClientOptions = deepgramClientOptions;
            _httpClient = httpClient.ConfigureDeepgram(_deepgramClientOptions);
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
                Log.HttpRequestException(_logger, "GET", uriSegment, hre);
                throw new HttpRequestException($"Error occurred during GET request to {uriSegment}: Message{hre.Message}", hre);
            }
            catch (Exception ex)
            {
                Log.Exception(_logger, "GET", ex.GetType().Name, ex);
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
                Log.HttpRequestException(_logger, "POST", uriSegment, hre);
                throw new HttpRequestException($"Error occurred during GET request to {uriSegment}: Message{hre.Message}", hre);
            }
            catch (Exception ex)
            {
                Log.Exception(_logger, "POST", ex.GetType().Name, ex);
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
                Log.HttpRequestException(_logger, "POST", uriSegment, hre);
                throw new HttpRequestException($"Error occurred during GET request to {uriSegment}: Message{hre.Message}", hre);
            }
            catch (Exception ex)
            {
                Log.Exception(_logger, "POST", ex.GetType().Name, ex);
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
                var response = await _httpClient.DeleteAsync(uriSegment);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException hre)
            {
                Log.HttpRequestException(_logger, "DELETE", uriSegment, hre);
                throw new HttpRequestException($"Error occurred during DELETE request to {uriSegment}: Message{hre.Message}", hre);
            }
            catch (Exception ex)
            {
                Log.Exception(_logger, "DELETE", ex.GetType().Name, ex);
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
                Log.HttpRequestException(_logger, "DELETE ASYNC", uriSegment, hre);
                throw new HttpRequestException($"Error occurred during DELETE ASYNC request to {uriSegment}: Message{hre.Message}", hre);
            }
            catch (Exception ex)
            {
                Log.Exception(_logger, "DELETE ASYNC", ex.GetType().Name, ex);
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
                Log.HttpRequestException(_logger, "PATCH", uriSegment, hre);
                throw new HttpRequestException($"Error occurred during PATCH request to {uriSegment}: Message{hre.Message}", hre);
            }
            catch (Exception ex)
            {
                Log.Exception(_logger, "PATCH", ex.GetType().Name, ex);
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
                Log.HttpRequestException(_logger, "PUT", uriSegment, hre);
                throw new HttpRequestException($"Error occurred during PUT request to {uriSegment}: Message{hre.Message}", hre);
            }
            catch (Exception ex)
            {
                Log.Exception(_logger, "PUT", ex.GetType().Name, ex);
                throw new Exception($"Error occurred during PUT request to {uriSegment}: Message {ex.Message} ", ex);
            }
        }



        internal void CheckForTimeout()
        {
            if (_timeout != null)
                _httpClient.Timeout = (TimeSpan)_timeout;
        }

        /// <summary>
        /// Set the time out on the HttpClient
        /// </summary>
        /// <param name="timeSpan"></param>
        public void SetTimeout(TimeSpan timeSpan)
            => _timeout = timeSpan;

    }
}
