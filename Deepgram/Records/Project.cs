namespace Deepgram.Records;

public record Project
{
    /// <summary>
    /// Unique identifier of the Deepgram project
    /// </summary>
    [JsonPropertyName("project_id")]
    public string? ProjectId { get; set; }

    /// <summary>
    /// Name of the Deepgram project
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

