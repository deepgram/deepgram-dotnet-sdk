using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Deepgram.Interfaces;

namespace Deepgram.Utillities
{
    public class HttpClientUtil : IHttpClientUtil
    {
        // Global client used in all instance when needed
        private static HttpClient _httpClient = Create();
        static HttpClientUtil() { }
        private static HttpClient Create()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgentUtil.GetUserAgent());
            return httpClient;
        }

        //Testable Wrapper around the httpClient
        public HttpClient GetHttpClient() { return _httpClient; }

        public static void SetTimeOut(TimeSpan timeSpan) =>
          _httpClient.Timeout = timeSpan;
    }
}
