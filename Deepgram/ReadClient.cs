namespace Deepgram;

/// <summary>
///  Client containing methods for interacting with Read API's
/// </summary>
/// <param name="httpClient"><see cref="HttpClient"/> for making Rest calls</param>
/// <param name="deepgramClientOptions"><see cref="DeepgramClientOptions"/> for HttpClient Configuration</param>
public class ReadClient(DeepgramClientOptions deepgramClientOptions, HttpClient httpClient)
    : AbstractRestClient(deepgramClientOptions, httpClient)
{
    /// <summary>
    /// Constructor with default Options
    /// </summary>
    /// <param name="apiKey">The key to authenticate with Deepgram</param>
    /// <param name="httpClient"><see cref="HttpClient"/> for making Rest calls</param>
    public ReadClient(string apiKey, HttpClient httpClient) : this(new DeepgramClientOptions(apiKey), httpClient) { }

    internal readonly string UrlPrefix = $"/{Constants.Defaults.API_VERSION}/{UriSegments.PROJECTS}";


}
