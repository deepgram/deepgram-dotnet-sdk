namespace Deepgram.Models.Responses;
public class ListOnPremCredentialsResponse
{
    [JsonPropertyName("distribution_credentials")]
    public List<OnPremCredentialResponse>? DistributionCredentials { get; set; }
}