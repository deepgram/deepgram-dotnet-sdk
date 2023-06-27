using Deepgram.Models;
using Deepgram.Request;
using Deepgram.Utilities;

namespace Deepgram.Clients
{
    internal abstract class BaseClient
    {
        internal Credentials _credentials;
        internal ApiRequest _apiRequest;

        public BaseClient(Credentials credentials)
        {
            _apiRequest = new ApiRequest(HttpClientUtil.HttpClient);
        }

    }
}
