using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Deepgram.Models;
using Deepgram.Utillities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Deepgram.Request
{
    public class ApiRequest
    {
        internal async Task<T> SendHttpRequestAsync<T>(HttpRequestMessage request)
        {
            var json = (await SendHttpRequestAsync(request)).JsonResponse;
            return JsonConvert.DeserializeObject<T>(json);
        }

        internal async Task<DeepgramResponse> SendHttpRequestAsync(HttpRequestMessage request)
        {
            var logger = Logger.LogProvider.GetLogger(typeof(ApiRequest).Name);
            logger.LogDebug($"{nameof(SendHttpRequestAsync)}: {request.RequestUri}");

            // uses the Wrapper around the httpclient so method can be tested
            var httpClient = new HttpClientUtil().GetHttpClient();
            var response = await httpClient.SendAsync(request);
            var json = await GetJsonStringFromResponse(response);

            return GetDeepgramResponseFromJson(logger, response, json);
        }

        private static DeepgramResponse GetDeepgramResponseFromJson(ILogger logger, HttpResponseMessage response, string json)
        {
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

        private static async Task<string> GetJsonStringFromResponse(HttpResponseMessage response)
        {
            var stream = await response.Content.ReadAsStreamAsync();
            string json;
            using (var sr = new StreamReader(stream))
            {
                json = await sr.ReadToEndAsync();
            }

            return json;
        }



    }
}
