namespace Deepgram.Models.Manage.v1;
public record GetProjectKeysResponse
{
    /// <summary>
    /// List of Deepgram api keys
    /// </summary>
    [JsonPropertyName("api_keys")]
    public IReadOnlyList<GetProjectKeyResponse>? ApiKeys { get; set; }
}
