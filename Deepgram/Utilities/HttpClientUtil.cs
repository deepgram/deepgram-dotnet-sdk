using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Deepgram.Utilities
{
    public class HttpClientUtil
    {
        // Client used in instance when needed
        internal HttpClient HttpClient { get; private set; }

        internal HttpClientUtil() {
            HttpClient = Create();
        }

        /// <summary>
        /// Create a Httpclient set common headers
        /// </summary>
        /// <returns></returns>
        private HttpClient Create()
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
        public void SetTimeOut(TimeSpan timeSpan)
        {
            HttpClient.Timeout = timeSpan;
        }
    }
}
