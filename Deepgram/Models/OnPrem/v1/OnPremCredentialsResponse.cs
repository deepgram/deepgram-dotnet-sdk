namespace Deepgram.Models.OnPrem.v1;

public record OnPremCredentialsResponse

{
    [JsonPropertyName("member")]
    public Member? Member { get; set; }

    [JsonPropertyName("distribution_credentials")]
    public DistributionCredentials? DistributionCredentials { get; set; }
}
