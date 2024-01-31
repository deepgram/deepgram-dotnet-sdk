namespace Deepgram.Models.Manage.v1;
public record GetProjectsResponse
{
    [JsonPropertyName("projects")]
    public IReadOnlyList<Project>? Projects { get; set; }

}
