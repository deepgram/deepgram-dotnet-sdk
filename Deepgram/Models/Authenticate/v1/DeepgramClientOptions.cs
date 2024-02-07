
namespace Deepgram.Models.Authenticate.v1;

/// <summary>
/// Configuration for the Deepgram client
/// </summary>
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

    public string? BaseAddress { get; set; } = "api.deepgram.com";

    /// <summary>
    /// Api endpoint version
    /// </summary>
    public string? APIVersion { get; set; } = "v1";


}
