namespace Deepgram.Models.Schemas;
public class UpdateProjectSchema
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("company")]
    public string? Company { get; set; }

    [JsonPropertyName("additional_properties")]
    Dictionary<string, string>? AdditionalProperties { get; set; }
}

