using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Deepgram.Extensions;
using Deepgram.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Deepgram.Request
{
    public class ApiRequest
    {
        static HttpClient httpClient = HttpClientExtension.Create();

        const string LOGGER_CATEGORY = "Deepgram.Request.ApiRequest";

        internal async Task<T> SendHttpRequestAsync<T>(HttpRequestMessage request)
        {
            var json = (await SendHttpRequestAsync(request)).JsonResponse;
            return JsonConvert.DeserializeObject<T>(json);
        }

        internal async Task<DeepgramResponse> SendHttpRequestAsync(HttpRequestMessage request)
        {
            var logger = Logger.LogProvider.GetLogger(LOGGER_CATEGORY);
            logger.LogDebug($"SendHttpRequestAsync: {request.RequestUri}");

            var response = await httpClient.SendAsync(request);

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

        public static void SetTimeOut(TimeSpan timeSpan) =>
            httpClient.Timeout = timeSpan;


    }
}
