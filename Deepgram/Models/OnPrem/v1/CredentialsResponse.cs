namespace Deepgram.Models.OnPrem.v1;
public record CredentialsResponse
{
    [JsonPropertyName("distribution_credentials")]
    public IReadOnlyList<CredentialResponse> DistributionCredentials { get; set; }
}
