namespace Deepgram.Models;

public class ApiKey
{
    /// <summary>
    /// Unique identifier of the Deepgram API key
    /// </summary>
    [JsonPropertyName("api_key_id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Comment for the Deepgram API key
    /// </summary>
    [JsonPropertyName("comment")]
    public string Comment { get; set; } = string.Empty;

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }

    [JsonPropertyName("scopes")]
    public string[] Scopes { get; set; }

    [JsonPropertyName("tags")]
    public string[] Tags { get; set; } = Array.Empty<string>();

    [JsonPropertyName("expiration_date")]
    public DateTime? ExpirationDate { get; set; }
}
