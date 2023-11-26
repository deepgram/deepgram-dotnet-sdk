namespace Deepgram.Models.Options;
public class DeepgramClientOptions
{
    /// <summary>
    /// BaseAddress of the server :defaults to https://api.deepgram.com
    /// </summary>
    public string? BaseAddress { get; set; } = Constants.HTTPCLIENT_NAME;

    /// <summary>
    /// proxy address include port if used
    /// </summary>
    public RestProxy? Proxy { get; set; }

    public string? Cache { get; set; }
    /// <summary>
    /// Timeout of client
    /// </summary>
    public int? TimeoutInSeconds { get; set; }

    /// <summary>
    /// Additional headers 
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }
}


