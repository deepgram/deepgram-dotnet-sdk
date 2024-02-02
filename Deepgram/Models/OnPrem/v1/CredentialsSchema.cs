namespace Deepgram.Models.OnPrem.v1;

public class CredentialsSchema
{
    /// <summary>
    /// comment to credentials
    /// </summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    /// <summary>
    /// scopes of credentials
    /// </summary>
    [JsonPropertyName("scopes")]
    public List<string>? Scopes { get; set; }

    /// <summary>
    /// provider
    /// </summary>
    [JsonPropertyName("provider")]
    public string? Provider { get; set; }
}
