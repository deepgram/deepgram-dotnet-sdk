namespace Deepgram.Models.Manage.v1;

public record Model
{
    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("model_id")]
    public string? ModelId { get; set; }
}

