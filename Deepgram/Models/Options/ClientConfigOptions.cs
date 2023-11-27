using System.Net;

namespace Deepgram.Models.Options;
public class ClientConfigOptions
{

    /// <summary>
    /// proxy address include port if used
    /// </summary>
    public WebProxy? Proxy { get; set; }

    public string? Cache { get; set; }

}


