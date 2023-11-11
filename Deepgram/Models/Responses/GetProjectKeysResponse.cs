namespace Deepgram.Models.Responses;
public class GetProjectKeysResponse
{
    [JsonPropertyName("api_keys")]
    public GetProjectKeyResponse[]? ApiKeys { get; set; }
}
