namespace Deepgram.Models;

public class Key
{
    /// <summary>
    /// member object
    /// </summary>
    [JsonPropertyName("member")]
    public KeyMember Member { get; set; }

    /// <summary>
    /// api key object
    /// </summary>
    [JsonPropertyName("api_key")]
    public ApiKey ApiKey { get; set; }
}
