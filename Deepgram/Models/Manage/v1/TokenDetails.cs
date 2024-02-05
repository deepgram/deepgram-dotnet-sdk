namespace Deepgram.Models.Manage.v1;

public record TokenDetails
{
    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("feature")]
    public string? Feature { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("input")]
    public int? Input { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("output")]
    public int? Output { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }
}
