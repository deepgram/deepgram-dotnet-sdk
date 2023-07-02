namespace Deepgram.Interfaces;

public interface IBaseClient
{
    IApiRequest ApiRequest { get; set; }
    CleanCredentials Credentials { get; set; }
    IRequestMessageBuilder RequestMessageBuilder { get; set; }
}
