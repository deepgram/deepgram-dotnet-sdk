using Deepgram.Transcription;
using System.Net.Http;
using System.Threading.Tasks;

namespace Deepgram.Request
{
    internal interface IApiRequest
    {
        Task<T> DoRequestAsync<T>(HttpMethod method, string uri, CleanCredentials credentials, object body = null, object queryParameters = null, object bodyObject = null);
        Task<T> DoStreamRequestAsync<T>(HttpMethod method, string uri, CleanCredentials credentials, StreamSource streamSource, object queryParameters = null);
    }
}
