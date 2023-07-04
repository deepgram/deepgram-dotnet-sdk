using System.Net.Http;
using System.Threading.Tasks;
using Deepgram.Transcription;

namespace Deepgram.Request
{
    public interface IApiRequest
    {
        IRequestMessageBuilder _requestMessageBuilder { get; set; }
        Task<T> DoRequestAsync<T>(HttpMethod method, string uri, CleanCredentials credentials, object body = null, object queryParameters = null);
        Task<T> DoStreamRequestAsync<T>(HttpMethod method, string uri, CleanCredentials credentials, StreamSource streamSource, object queryParameters = null);
    }
}
