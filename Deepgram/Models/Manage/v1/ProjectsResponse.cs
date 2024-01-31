namespace Deepgram.Models.Manage.v1;
public record ProjectsResponse
{
    [JsonPropertyName("projects")]
    public IReadOnlyList<Project>? Projects { get; set; }

}
