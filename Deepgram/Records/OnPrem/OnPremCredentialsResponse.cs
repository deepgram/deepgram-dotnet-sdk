namespace Deepgram.Records.OnPrem;

public record OnPremCredentialsResponse

{
    [JsonPropertyName("member")]
    public Member? Member { get; set; }

    [JsonPropertyName("distribution_credentials")]
    public DistributionCredentials? DistributionCredentials { get; set; }
}
