namespace Deepgram.Request;

public class RequestMessageBuilder : IRequestMessageBuilder
{
    /// <summary>
    /// Creates a Http Request Message for the Api calls
    /// </summary>
    /// <param name="method"></param>
    /// <param name="uri"></param>
    /// <param name="credentials"></param>
    /// <param name="body"></param>
    /// <param name="queryParameters"></param>
    /// <returns></returns>
    public HttpRequestMessage CreateHttpRequestMessage(HttpMethod method, string uri, CleanCredentials credentials, object? body = null, object? queryParameters = null)
    {
        var req = ConfigureHttpRequestMessage(method, uri, credentials, queryParameters);

        if (body != null)
        {
            var payload = JsonConvert.SerializeObject(body, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            req.Content = new StringContent(payload, Encoding.UTF8, "application/json");
        }
        return req;
    }

    /// <summary>
    /// Create a HttpRequestMessage when a stream is the body source
    /// </summary>
    /// <param name="method"></param>
    /// <param name="uri"></param>
    /// <param name="credentials"></param>
    /// <param name="streamSource"></param>
    /// <param name="queryParameters"></param>
    /// <returns></returns>
    public HttpRequestMessage CreateStreamHttpRequestMessage(HttpMethod method, string uri, CleanCredentials credentials, StreamSource streamSource, object? queryParameters = null)
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

    private static HttpRequestMessage ConfigureHttpRequestMessage(HttpMethod method, string uri, CleanCredentials credentials, object? queryParameters = null)
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
