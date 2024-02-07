namespace Deepgram.Models.OnPrem.v1;

public record CredentialResponse
{
    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("member")]
    public Member? Member { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("distribution_credentials")]
    public DistributionCredentials? DistributionCredentials { get; set; }
}
