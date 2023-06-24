using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
        Credentials _credentials { get; set; }
        public ApiRequest(Credentials credentials)
        {
            _credentials = credentials;
        }

        const string LOGGER_CATEGORY = "Deepgram.Request.ApiRequest";


        public async Task<T> DoRequestAsync<T>(HttpMethod method, string uri, object body = null, object queryParameters = null)
        {
            var req = ConfigureHttpRequestMessage(method, uri, queryParameters);
            SetContent(ref req, body);
            return await SendHttpRequestAsync<T>(req);
        }

        public async Task<T> DoStreamRequestAsync<T>(HttpMethod method, string uri, StreamSource streamSource, object queryParameters = null)
        {
            var req = ConfigureHttpRequestMessage(method, uri, queryParameters);
            SetContentAsStream(ref req, streamSource);
            return await SendHttpRequestAsync<T>(req);
        }

        internal HttpRequestMessage ConfigureHttpRequestMessage(HttpMethod method, string uri, object queryParameters = null)
        {
            var requestUri = GetUriWithQuerystring(uri, queryParameters);

            var req = new HttpRequestMessage
            {
                RequestUri = requestUri,
                Method = method
            };
            SetAuthentication(ref req);

            return req;
        }

        private Uri GetUriWithQuerystring(string uriSegment, object queryParameters = null) =>
           UriExtension.ResolveUri(
               _credentials.ApiUrl, uriSegment,
               Convert.ToBoolean(_credentials.RequireSSL) ? "https" : "http",
               queryParameters);

        private void SetAuthentication(ref HttpRequestMessage request) =>
            request.Headers.Authorization = new AuthenticationHeaderValue("token", _credentials.ApiKey);

        private void SetContent(ref HttpRequestMessage request, object bodyObject)
        {
            if (null != bodyObject)
            {
                var payload = JsonConvert.SerializeObject(bodyObject, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
            }
        }

        private void SetContentAsStream(ref HttpRequestMessage request, StreamSource streamSource)
        {
            Stream stream = streamSource.Stream;
            stream.Seek(0, SeekOrigin.Begin);
            HttpContent httpContent = new StreamContent(stream);
            httpContent.Headers.Add("Content-Type", streamSource.MimeType);
            httpContent.Headers.Add("Content-Length", stream.Length.ToString());
            request.Content = httpContent;
        }

        private async Task<T> SendHttpRequestAsync<T>(HttpRequestMessage request)
        {
            var json = (await SendHttpRequestAsync(request)).JsonResponse;
            return JsonConvert.DeserializeObject<T>(json);
        }

        private async Task<DeepgramResponse> SendHttpRequestAsync(HttpRequestMessage request)
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
