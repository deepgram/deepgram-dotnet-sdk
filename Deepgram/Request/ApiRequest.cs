using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Deepgram.Common;
using Deepgram.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Deepgram.Request
{
    public class ApiRequest
    {
        private Credentials _credentials;
        private TimeSpan _timeout;
        public ApiRequest(Credentials credentials, TimeSpan timeSpan)
        {
            _timeout = timeSpan;
            _credentials = credentials;
        }
        const string LOGGER_CATEGORY = "Deepgram.Request.ApiRequest";

        private void SetHeaders(ref HttpRequestMessage request)
        {
            request.Headers.Add("Accept", "application/json");
            SetUserAgent(ref request);
            SetCredentials(ref request);
        }

        private void SetUserAgent(ref HttpRequestMessage request)
        {
            var userAgent = Helpers.GetUserAgent();
            request.Headers.UserAgent.ParseAdd(userAgent);
        }

        private void SetCredentials(ref HttpRequestMessage request)
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("token", _credentials.ApiKey);
        }

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

        internal async Task<T> DoRequestAsync<T>(HttpMethod method, string uri, object queryParameters = null, object bodyObject = null)
        {
            var requestUri = GetUriWithQuerystring(uri, queryParameters);

            var req = new HttpRequestMessage
            {
                RequestUri = requestUri,
                Method = method
            };
            SetHeaders(ref req);
            SetContent(ref req, bodyObject);

            return await SendHttpRequestAsync<T>(req);
        }

        internal async Task<T> DoStreamRequestAsync<T>(HttpMethod method, string uri, StreamSource streamSource, object queryParameters = null)
        {
            var requestUri = GetUriWithQuerystring(uri, queryParameters);

            var req = new HttpRequestMessage
            {
                RequestUri = requestUri,
                Method = method
            };
            SetHeaders(ref req);
            SetContentAsStream(ref req, streamSource);

            return await SendHttpRequestAsync<T>(req);
        }

        private Uri GetUriWithQuerystring(string uri, object queryParameters = null)
        {
            string protocol = Convert.ToBoolean(_credentials.RequireSSL) ? "https" : "http";
            if (null != queryParameters)
            {
                var querystring = Helpers.GetParameters(queryParameters);
                return new Uri($"{protocol}://{_credentials.ApiUrl}{uri}?{querystring}");
            }
            return new Uri($"{protocol}://{_credentials.ApiUrl}{uri}");
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

            var httpClient = GetHttpClient();
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

        private HttpClient GetHttpClient()
        {


            var httpClient = new HttpClient();
            if (_timeout == TimeSpan.Zero)
                httpClient.Timeout = _timeout;

            return httpClient;
        }
    }
}
