using Deepgram.Request;
using Deepgram.Utilities;

namespace Deepgram.Common
{
    public abstract class BaseClient : IBaseClient
    {
        public IApiRequest ApiRequest { get; set; }
        public CleanCredentials Credentials { get; set; }


        public BaseClient(CleanCredentials credentials)
        {
            ApiRequest = new ApiRequest(HttpClientUtil.HttpClient);
            Credentials = credentials;

        }
    }
}
