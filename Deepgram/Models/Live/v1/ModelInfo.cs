namespace Deepgram.Models.Live.v1;

public record ModelInfo
{
    /// <summary>
    /// Architecture of the model
    /// </summary>
    [JsonPropertyName("arch")]
    public string? Arch { get; set; }

    /// <summary>
    /// Name of the model
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Version of the model
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }
}
