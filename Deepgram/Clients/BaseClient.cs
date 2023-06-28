using Deepgram.Interfaces;
using Deepgram.Models;
using Deepgram.Request;
using Deepgram.Utilities;

namespace Deepgram.Clients
{
    public abstract class BaseClient
    {
        internal Credentials _credentials;
        internal IApiRequest _apiRequest;
        internal RequestMessageBuilder _requestMessageBuilder;

        public BaseClient(Credentials credentials)
        {
            _credentials = credentials;
            _apiRequest = new ApiRequest(HttpClientUtil.HttpClient);
            _requestMessageBuilder = new RequestMessageBuilder();
        }

    }
}
