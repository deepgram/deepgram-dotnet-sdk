using System.Net.Http;

namespace Deepgram.Interfaces
{
    public interface IHttpClientUtil
    {
        HttpClient GetHttpClient();

    }
}
