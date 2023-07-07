namespace Deepgram.Interfaces;

public interface IBaseClient
{
    internal IApiRequest ApiRequest { get; set; }
    internal CleanCredentials Credentials { get; set; }
    internal IRequestMessageBuilder RequestMessageBuilder { get; set; }
}
