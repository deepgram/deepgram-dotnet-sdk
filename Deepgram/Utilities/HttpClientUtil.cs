using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Deepgram.Utilities
{

    public class HttpClientUtil
    {
        // Global client used in all instance when needed
        internal static HttpClient HttpClient = Create();
        static HttpClientUtil() { }

        /// <summary>
        /// Create a Httpclient set common headers
        /// </summary>
        /// <returns></returns>
        private static HttpClient Create()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgentUtil.GetUserAgent());

            return httpClient;
        }

        /// <summary>
        /// sets timeout on the httpclient
        /// </summary>
        /// <param name="timeSpan"></param>
        public static void SetTimeOut(TimeSpan timeSpan) =>
          HttpClient.Timeout = timeSpan;


    }
}
