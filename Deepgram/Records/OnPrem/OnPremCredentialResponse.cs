namespace Deepgram.Records.OnPrem;

public record OnPremCredentialResponse
{
    [JsonPropertyName("member")]
    public Member? Member { get; set; }

    [JsonPropertyName("distribution_credentials")]
    public DistributionCredentials? DistributionCredentials { get; set; }
}
