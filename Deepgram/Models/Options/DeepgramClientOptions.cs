namespace Deepgram.Models.Options;
public class DeepgramClientOptions
{
    /// <summary>
    /// Timeout of client
    /// </summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Additional headers 
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// BaseAddress of the server :defaults to api.deepgram.com
    /// no need to attach the protocol it will be added internally
    /// </summary>
    public string BaseAddress { get; set; } = Constants.DEFAULT_URI;
}
