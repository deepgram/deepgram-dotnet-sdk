namespace Deepgram.Models.Options;
public class DeepgramClientOptions
{
    /// <summary>
    /// Additional headers 
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// BaseAddress of the server :defaults to api.deepgram.com
    /// no need to attach the protocol it will be added internally
    /// </summary>
    public string BaseAddress { get; set; } = Constants.DEFAULT_URI;

    internal string ApiKey { get; }

    /// <summary>
    /// Creates a new Deepgram client options
    /// </summary>
    /// <param name="apiKey">The key to authenticate with deepgram</param>
    public DeepgramClientOptions(string apiKey)
    {
        ApiKey = apiKey;
    }
}
