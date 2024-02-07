namespace Deepgram.Models.OnPrem.v1;

public record DistributionCredentials
{
    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("distribution_credentials_id")]
    public string? DistributionCredentialsId { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("provider")]
    public string? Provider { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("scopes")]
    public IReadOnlyList<string>? Scopes { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("created")]
    public DateTime? Created { get; set; }
}
