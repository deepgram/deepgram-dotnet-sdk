namespace Deepgram.Models.OnPrem.v1;

public record CredentialsResponse
{
    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("distribution_credentials")]
    public IReadOnlyList<CredentialResponse>? DistributionCredentials { get; set; }
}
