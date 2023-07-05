namespace Deepgram.Models;

public class UsageRequestResponse
{
    /// <summary>
    /// Details of the request
    /// </summary>
    [JsonPropertyName("details")]
    public UsageRequestResponseDetail Details { get; set; }

    /// <summary>
    /// If the request failed, this will contain the error message.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; }
}
