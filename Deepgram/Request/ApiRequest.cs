using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Deepgram.Common;
using Deepgram.Extensions;
using Deepgram.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Deepgram.Request
{
    public class ApiRequest
    {
        const string LOGGER_CATEGORY = "Deepgram.Request.ApiRequest";

        private static void SetHeaders(ref HttpRequestMessage request, Credentials credentials)
        {
            request.Headers.Add("Accept", "application/json");
            SetUserAgent(ref request);
            SetCredentials(ref request, credentials);
        }

        private static void SetUserAgent(ref HttpRequestMessage request)
        {
            var userAgent = Helpers.GetUserAgent();
            request.Headers.UserAgent.ParseAdd(userAgent);
        }

        private static void SetCredentials(ref HttpRequestMessage request, Credentials credentials)
        {
            var apiKey = credentials.ApiKey;
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("token", apiKey);
        }

        private static void SetContent(ref HttpRequestMessage request, object bodyObject)
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

        private static void SetContentAsStream(ref HttpRequestMessage request, StreamSource streamSource)
        {
            Stream stream = streamSource.Stream;
            stream.Seek(0, SeekOrigin.Begin);
            HttpContent httpContent = new StreamContent(stream);
            httpContent.Headers.Add("Content-Type", streamSource.MimeType);
            httpContent.Headers.Add("Content-Length", stream.Length.ToString());
            request.Content = httpContent;
        }

        internal static async Task<T> DoRequestAsync<T>(HttpMethod method, string uri, Credentials credentials, object queryParameters = null, object bodyObject = null)
        {
            var requestUri = GetUriWithQuerystring(credentials, uri, queryParameters);

            var req = new HttpRequestMessage
            {
                RequestUri = requestUri,
                Method = method
            };
            SetHeaders(ref req, credentials);
            SetContent(ref req, bodyObject);

            return await SendHttpRequestAsync<T>(req);
        }

        internal static async Task<T> DoStreamRequestAsync<T>(HttpMethod method, string uri, Credentials credentials, StreamSource streamSource, object queryParameters = null)
        {
            var requestUri = GetUriWithQuerystring(credentials, uri, queryParameters);

            var req = new HttpRequestMessage
            {
                RequestUri = requestUri,
                Method = method
            };
            SetHeaders(ref req, credentials);
            SetContentAsStream(ref req, streamSource);

            return await SendHttpRequestAsync<T>(req);
        }

        private static Uri GetUriWithQuerystring(Credentials _credentials, string uri, object queryParameters = null) =>
       UriExtension.ResolveUri(
        _credentials, uri,
           Convert.ToBoolean(_credentials.RequireSSL) ? "https" : "http",
           queryParameters);


        private static async Task<T> SendHttpRequestAsync<T>(HttpRequestMessage request)
        {
            var json = (await SendHttpRequestAsync(request)).JsonResponse;
            return JsonConvert.DeserializeObject<T>(json);
        }

        private static async Task<DeepgramResponse> SendHttpRequestAsync(HttpRequestMessage request)
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

        private static HttpClient GetHttpClient()
        {


            var httpClient = new HttpClient();
            if (Config.HttpClientTimeOut == TimeSpan.Zero)
                httpClient.Timeout = Config.HttpClientTimeOut;

            return httpClient;
        }
    }
}
