namespace Deepgram.Records;
public record GetProjectKeysResponse
{
    [JsonPropertyName("api_keys")]
    public IReadOnlyList<GetProjectKeyResponse>? ApiKeys { get; set; }
}
