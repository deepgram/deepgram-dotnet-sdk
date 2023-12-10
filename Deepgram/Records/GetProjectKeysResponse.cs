namespace Deepgram.Records;
public record GetProjectKeysResponse
{
    /// <summary>
    /// List of Deepgram api keys
    /// </summary>
    [JsonPropertyName("api_keys")]
    public IReadOnlyList<GetProjectKeyResponse>? ApiKeys { get; set; }
}
