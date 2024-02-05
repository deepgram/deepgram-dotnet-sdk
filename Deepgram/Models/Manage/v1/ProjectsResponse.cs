namespace Deepgram.Models.Manage.v1;
public record ProjectsResponse
{
    /// <summary>
    /// List of Projects
    /// </summary>
    [JsonPropertyName("projects")]
    public IReadOnlyList<ProjectResponse>? Projects { get; set; }
}
