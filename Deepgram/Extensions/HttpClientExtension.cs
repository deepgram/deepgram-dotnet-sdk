using System.Net.Http;
using System.Net.Http.Headers;

namespace Deepgram.Extensions
{
    public static class HttpClientExtension
    {
        public static HttpClient Create()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(Common.UserAgentHelper.GetUserAgent());
            return httpClient;
        }
    }
}
