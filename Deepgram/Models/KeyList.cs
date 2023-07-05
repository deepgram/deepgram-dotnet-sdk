

namespace Deepgram.Models;

public class KeyList
{
    /// <summary>
    /// List of Deepgram api keys
    /// </summary>
    [JsonPropertyName("api_keys")]
    public Key[] ApiKeys { get; set; }
}
