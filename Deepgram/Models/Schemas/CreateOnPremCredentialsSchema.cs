namespace Deepgram.Models.Schemas;

public class CreateOnPremCredentialsSchema
{
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("scopes")]
    public string[]? Scopes { get; set; }

    [JsonPropertyName("provider")]
    public string? Provider { get; set; }
}