namespace Deepgram.Models.Schemas;
public class SendProjectInviteSchema
{
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }

    [JsonPropertyName("additional_properties")]
    Dictionary<string, string>? AdditionalProperties { get; set; }
}
