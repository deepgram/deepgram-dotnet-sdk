namespace Deepgram.Models.Responses;

public class GetProjectKeyResponse
{
    [JsonPropertyName("member")]
    public Member? Member { get; set; }

    [JsonPropertyName("api_key")]
    public Key? ApiKey { get; set; }
}
