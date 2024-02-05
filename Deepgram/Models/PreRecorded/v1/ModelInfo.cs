namespace Deepgram.Models.PreRecorded.v1;

public record ModelInfo
{
    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("arch")]
    public string? Arch { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }
}
