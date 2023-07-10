using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Deepgram.Transcription;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Deepgram.Request
{
    public class ApiRequest
    {
        readonly HttpClient _httpClient;
        readonly CleanCredentials _cleanCredentials;
        readonly RequestMessageBuilder _messageBuilder;
        internal ApiRequest(HttpClient httpClient, CleanCredentials credentials)
        {
            _httpClient = httpClient;
            _cleanCredentials = credentials;
            _messageBuilder = new RequestMessageBuilder();
        }

        const string LOGGER_CATEGORY = "Deepgram.Request.ApiRequest";



        public async Task<T> DoRequestAsync<T>(HttpMethod method, string uri, object body = null, object queryParameters = null)
        {
            var req = _messageBuilder.CreateHttpRequestMessage(method, uri, _cleanCredentials, body, queryParameters);

            return await SendHttpRequestAsync<T>(req);
        }

        public async Task<T> DoStreamRequestAsync<T>(HttpMethod method, string uri, StreamSource streamSource, object queryParameters = null)
        {
            var req = _messageBuilder.CreateStreamHttpRequestMessage(method, uri, _cleanCredentials, streamSource, queryParameters);

            return await SendHttpRequestAsync<T>(req);
        }

        internal virtual async Task<T> SendHttpRequestAsync<T>(HttpRequestMessage request)
        {
            var json = (await SendHttpRequestAsync(request)).JsonResponse;
            return JsonConvert.DeserializeObject<T>(json);
        }

        private async Task<DeepgramResponse> SendHttpRequestAsync(HttpRequestMessage request)
        {
            var logger = Logger.LogProvider.GetLogger(LOGGER_CATEGORY);
            logger.LogDebug($"SendHttpRequestAsync: {request.RequestUri}");
            var response = await _httpClient.SendAsync(request);
            var stream = await response.Content.ReadAsStreamAsync();
            string json;
            using (var sr = new StreamReader(stream))
            {
                json = await sr.ReadToEndAsync();
            }
            try
            {
                logger.LogDebug(json);
                response.EnsureSuccessStatusCode();
                return new DeepgramResponse
                {
                    Status = response.StatusCode,
                    JsonResponse = json
                };
            }
            catch (HttpRequestException exception)
            {
                logger.LogError($"FAIL: {response.StatusCode}");
                throw new DeepgramHttpRequestException(exception.Message) { HttpStatusCode = response.StatusCode, Json = json };
            }
        }
    }
}
