using Deepgram.Models;

namespace Deepgram.Interfaces
{
    public interface IBaseClient
    {
        IApiRequest ApiRequest { get; set; }
        Credentials Credentials { get; set; }
        IRequestMessageBuilder RequestMessageBuilder { get; set; }
    }
}
