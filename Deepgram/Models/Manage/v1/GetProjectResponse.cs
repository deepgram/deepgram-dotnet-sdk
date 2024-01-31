namespace Deepgram.Models.Manage.v1;
public record GetProjectResponse
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

    /// <summary>
    /// Name of the company associated with the Deepgram project
    /// </summary>
    [JsonPropertyName("company")]
    public string? Company { get; set; }
}
