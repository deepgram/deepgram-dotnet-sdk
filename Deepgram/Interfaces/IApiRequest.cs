using System.Net.Http;
using System.Threading.Tasks;

namespace Deepgram.Interfaces
{
    public interface IApiRequest
    {
        Task<T> SendHttpRequestAsync<T>(HttpRequestMessage request);
    }
}
