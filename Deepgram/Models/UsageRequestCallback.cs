namespace Deepgram.Models;

/// <summary>
///  If a callback was included in the request, but the request has not completed yet, 
///  then this object will exist, but it will be empty.
/// </summary>
public class UsageRequestCallback
{
    /// <summary>
    /// Status Code of the callback
    /// </summary>
    [JsonPropertyName("code")]
    public int? Code { get; set; }

    /// <summary>
    /// DateTime the callback completed
    /// </summary>
    [JsonPropertyName("completed")]
    public DateTime? Completed { get; set; }
}
