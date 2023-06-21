using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Deepgram.Models;
using Deepgram.Utilities;
using Newtonsoft.Json;

namespace Deepgram.Request
{
    internal static class RequestMessageBuilder
    {
        internal static HttpRequestMessage CreateHttpRequestMessage(HttpMethod method, string uri, Credentials credentials, object body = null, object queryParameters = null)
        {
            var req = ConfigureHttpRequestMessage(method, uri, credentials, queryParameters);

            if (null != body)
            {
                var payload = JsonConvert.SerializeObject(body, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                req.Content = new StringContent(payload, Encoding.UTF8, "application/json");
            }

            return req;

        }

        internal static HttpRequestMessage CreateStreamHttpRequestMessage(HttpMethod method, string uri, Credentials credentials, StreamSource streamSource, object queryParameters = null)
        {
            var req = ConfigureHttpRequestMessage(method, uri, credentials, queryParameters);

            Stream stream = streamSource.Stream;
            stream.Seek(0, SeekOrigin.Begin);
            HttpContent httpContent = new StreamContent(stream);
            httpContent.Headers.Add("Content-Type", streamSource.MimeType);
            httpContent.Headers.Add("Content-Length", stream.Length.ToString());
            req.Content = httpContent;

            return req;
        }

        private static HttpRequestMessage ConfigureHttpRequestMessage(HttpMethod method, string uri, Credentials credentials, object queryParameters = null)
        {
            var requestUri = UriUtil.ResolveUri(
                credentials.ApiUrl,
                uri,
                Convert.ToBoolean(credentials.RequireSSL) ? "https" : "http",
                queryParameters);

            var req = new HttpRequestMessage
            {
                RequestUri = requestUri,
                Method = method
            };

            req.Headers.Authorization = new AuthenticationHeaderValue("token", credentials.ApiKey);
            return req;
        }
    }
}
