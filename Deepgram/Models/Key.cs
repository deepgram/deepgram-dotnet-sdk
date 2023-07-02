namespace Deepgram.Models;

public class Key
{
    /// <summary>
    /// member object
    /// </summary>
    [JsonProperty("member")]
    public KeyMember Member { get; set; }

    /// <summary>
    /// api key object
    /// </summary>
    [JsonProperty("api_key")]
    public ApiKey ApiKey { get; set; }
}
