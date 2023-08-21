using Deepgram.Interfaces;
using Deepgram.Models;
using Deepgram.Request;
using Deepgram.Utilities;

namespace Deepgram.Clients
{
    public abstract class BaseClient : IBaseClient
    {
        public Credentials Credentials { get; set; }
        public IApiRequest ApiRequest { get; set; }
        public IRequestMessageBuilder RequestMessageBuilder { get; set; }

        protected HttpClientUtil HttpClientUtil { get; }

        public BaseClient(Credentials credentials, HttpClientUtil httpClientUtil)
        {
            HttpClientUtil = httpClientUtil;

            Credentials = credentials;
            ApiRequest = new ApiRequest(HttpClientUtil.HttpClient);
            RequestMessageBuilder = new RequestMessageBuilder();
        }

    }
}
