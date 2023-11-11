namespace Deepgram.Models;

public class DistributionCredentials
{
    [JsonPropertyName("distribution_credentials_id")]
    public string? DistributionCredentialsId { get; set; }

    [JsonPropertyName("provider")]
    public string? Provider { get; set; }

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("scopes")]
    public string[]? Scopes { get; set; }

    [JsonPropertyName("created")]
    public string? Created { get; set; }
}
