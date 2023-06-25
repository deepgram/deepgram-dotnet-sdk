using System.Net.Http;
using System.Threading.Tasks;

namespace Deepgram.Request;

public interface IApiRequest
{
    Task<T> SendHttpRequestAsync<T>(HttpRequestMessage request);
}
