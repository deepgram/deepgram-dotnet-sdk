namespace Deepgram.Models.Responses;

public class OnPremCredentialResponse
{
    [property: JsonPropertyName("member")]
    public Member? Member { get; set; }

    [JsonPropertyName("distribution_credentials")]
    public DistributionCredentials? DistributionCredentials { get; set; }
}
