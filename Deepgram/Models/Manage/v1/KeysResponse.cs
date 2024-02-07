namespace Deepgram.Models.Manage.v1;

public record KeysResponse
{
    /// <summary>
    /// List of Deepgram api keys
    /// </summary>
    [JsonPropertyName("api_keys")]
    public IReadOnlyList<KeyScopeResponse>? ApiKeys { get; set; }
}
