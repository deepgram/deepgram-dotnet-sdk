namespace Deepgram.Records.OnPrem;
public record ListOnPremCredentialsResponse
{
    [JsonPropertyName("distribution_credentials")]
    public IReadOnlyList<OnPremCredentialsResponse> DistributionCredentials { get; set; }
}
