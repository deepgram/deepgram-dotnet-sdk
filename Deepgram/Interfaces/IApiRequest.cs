using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Deepgram.Interfaces
{
    public interface IApiRequest
    {
        Task<T> SendHttpRequestAsync<T>(HttpRequestMessage request, CancellationToken token = new CancellationToken());
    }
}

