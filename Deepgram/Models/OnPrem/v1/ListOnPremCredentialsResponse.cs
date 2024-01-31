namespace Deepgram.Models.OnPrem.v1;
public record ListOnPremCredentialsResponse
{
    [JsonPropertyName("distribution_credentials")]
    public IReadOnlyList<OnPremCredentialsResponse> DistributionCredentials { get; set; }
}
