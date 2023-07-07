namespace Deepgram.Interfaces;

public interface IApiRequest
{
    Task<T> SendHttpRequestAsync<T>(HttpMethod method, string uri, object? body = null, object? queryParameters = null);
}
