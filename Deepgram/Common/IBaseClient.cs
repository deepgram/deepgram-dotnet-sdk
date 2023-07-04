using Deepgram.Request;

namespace Deepgram.Common
{
    public interface IBaseClient
    {
        IApiRequest ApiRequest { get; set; }
        CleanCredentials Credentials { get; set; }
    }
}
