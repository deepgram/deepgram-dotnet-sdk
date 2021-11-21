using System;
using System.Net;
using System.Reflection;
using System.Text;
using Deepgram.Transcription;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Deepgram.Request
{
    public class ApiRequest
    {
        const string LOGGER_CATEGORY = "Deepgram.Request.ApiRequest";
      
        private static void SetHeaders(ref HttpRequestMessage request, CleanCredentials credentials)
        {
            request.Headers.Add("Accept", "application/json");
            SetUserAgent(ref request);
            SetCredentials(ref request, credentials);
        }

        private static void SetUserAgent(ref HttpRequestMessage request)
        {
#if NETSTANDARD1_6 || NETSTANDARD2_0 || NETSTANDARD2_1
            // TODO: watch the next core release; may have functionality to make this cleaner
            var languageVersion = (System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription)
                .Replace(" ", "")
                .Replace("/", "")
                .Replace(":", "")
                .Replace(";", "")
                .Replace("_", "")
                .Replace("(", "")
                .Replace(")", "")
                ;
#else
            var languageVersion = System.Diagnostics.FileVersionInfo
                .GetVersionInfo(typeof(int).Assembly.Location)
                .ProductVersion;
#endif
            var libraryVersion = typeof(ApiRequest)
                .GetTypeInfo()
                .Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;

            string userAgent = $"deepgram/{libraryVersion} dotnet/{languageVersion}";
            request.Headers.UserAgent.ParseAdd(userAgent);
        }

        private static void SetCredentials(ref HttpRequestMessage request, CleanCredentials credentials)
        {
            var apiKey = (credentials.ApiKey != null ? credentials.ApiKey : Configuration.Instance.Settings["appSettings:Deepgram.Api.Key"])?.ToLower();
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("token", apiKey);
        }

        private static void SetContent(ref HttpRequestMessage request, object? bodyObject)
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

        internal static async Task<T> DoRequestAsync<T>(HttpMethod method, Uri uri, CleanCredentials credentials, object? queryParameters = null, object? bodyObject = null)
        {
            uri = GetUriWithQueryString(uri, queryParameters);

            var req = new HttpRequestMessage
            {
                RequestUri = uri,
                Method = method
            };
            SetHeaders(ref req, credentials);
            SetContent(ref req, bodyObject);

            return await SendHttpRequestAsync<T>(req);
        }

        internal static async Task<T> DoRequestAsync<T>(HttpMethod method, Uri uri, CleanCredentials credentials, StreamSource streamSource, object? queryParameters = null)
        {
            uri = GetUriWithQueryString(uri, queryParameters);

            var req = new HttpRequestMessage
            {
                RequestUri = uri,
                Method = method
            };
            SetHeaders(ref req, credentials);
            SetContentAsStream(ref req, streamSource);

            return await SendHttpRequestAsync<T>(req);
        }

        private static Uri GetUriWithQueryString(Uri uri, object? queryParameters)
        {
            if (null != queryParameters)
            {
                var queryParams = GetParameters(queryParameters);
                var sb = new StringBuilder();
                foreach (var parameter in queryParams)
                {
                    sb.AppendFormat("{0}={1}&", WebUtility.UrlEncode(parameter.Key), WebUtility.UrlEncode(parameter.Value));
                }

                uri = new Uri($"{uri}?{sb.ToString()}");
            }
            return uri;
        }

        private static Dictionary<string, string> GetParameters(object parameters)
        {
            var json = JsonConvert.SerializeObject(parameters, new JsonSerializerSettings() { 
                NullValueHandling = NullValueHandling.Ignore });
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        private static async Task<T> SendHttpRequestAsync<T>(HttpRequestMessage request)
        {
            var json = (await SendHttpRequestAsync(request)).JsonResponse;
            return JsonConvert.DeserializeObject<T>(json);
        }

        private static async Task<DeepgramResponse> SendHttpRequestAsync(HttpRequestMessage request)
        {
            var logger = Logger.LogProvider.GetLogger(LOGGER_CATEGORY);
            logger.LogDebug($"SendHttpRequestAsync: {request.RequestUri}");
            var response = await Configuration.Instance.Client.SendAsync(request);
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
