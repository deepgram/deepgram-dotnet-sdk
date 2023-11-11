namespace Deepgram.Models.Options;

public class DeepgramClientOptions
{
    ///<summary>
    ///Optional headers for initializing the client.
    ///</summary>
    public Dictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// The URL used to interact with production, On-prem and other Deepgram environments. Defaults to `api.deepgram.com`.
    /// </summary>    
    public string? Url { get; set; }
    FetchOptions? FetchOptions { get; set; }

    RestProxy? RestProxy { get; set; }
}
