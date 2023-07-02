namespace Deepgram.Models;

public class CleanCredentials
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="apiKey">Deepgram API Key</param>
    /// <param name="apiUrl">Url of Deepgram API</param>
    /// <param name="requireSSL">Require SSL on requests</param>
    public CleanCredentials
        (string apiKey, string apiUrl, bool requireSSL)
    {
        ApiKey = apiKey;
        ApiUrl = apiUrl;
        RequireSSL = requireSSL;
    }

    /// <summary>
    /// Deepgram API Key
    /// </summary>
    public string ApiKey { get; set; }

    /// <summary>
    /// On-premise Url of the Deepgram API
    /// </summary>
    public string ApiUrl { get; set; }

    /// <summary>
    /// Require SSL on requests
    /// </summary>
    public bool RequireSSL { get; set; }
}
