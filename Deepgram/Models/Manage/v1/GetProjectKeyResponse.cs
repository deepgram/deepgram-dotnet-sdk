namespace Deepgram.Models.Manage.v1;

public record GetProjectKeyResponse
{
    /// <summary>
    /// member object <see cref="v1.Member"/>
    /// </summary>
    [JsonPropertyName("member")]
    public Member? Member { get; set; }

    /// <summary>
    /// api key object <see cref="Key"/>
    /// </summary>
    [JsonPropertyName("api_key")]
    public Key? ApiKey { get; set; }
}
