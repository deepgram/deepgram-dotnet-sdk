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

    /// <summary>
    /// If using named clients you can assign the Client Name here
    /// </summary>
    public string NamedClientName { get; set; }

    FetchOptions? FetchOptions { get; set; }
}
