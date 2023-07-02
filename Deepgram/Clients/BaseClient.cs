namespace Deepgram.Clients;

public abstract class BaseClient : IBaseClient
{
    public CleanCredentials Credentials { get; set; }
    public IApiRequest ApiRequest { get; set; }
    public IRequestMessageBuilder RequestMessageBuilder { get; set; }

    public BaseClient(CleanCredentials credentials)
    {
        Credentials = credentials;
        ApiRequest = new ApiRequest(HttpClientUtil.HttpClient);
        RequestMessageBuilder = new RequestMessageBuilder();
    }

}
