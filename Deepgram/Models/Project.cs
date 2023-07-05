namespace Deepgram.Models;

public class Project
{
    /// <summary>
    /// Unique identifier of the Deepgram project
    /// </summary>
    [JsonPropertyName("project_id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Name of the Deepgram project
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Name of the company associated with the Deepgram project
    /// </summary>
    [JsonPropertyName("company")]
    public string Company { get; set; }
}
