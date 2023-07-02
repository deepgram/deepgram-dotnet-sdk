namespace Deepgram.Interfaces;

public interface IApiRequest
{
    Task<T> SendHttpRequestAsync<T>(HttpRequestMessage request);
}
