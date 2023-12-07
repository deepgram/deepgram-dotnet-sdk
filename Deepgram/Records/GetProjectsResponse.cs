namespace Deepgram.Records;
public record GetProjectsResponse
{
    [JsonPropertyName("projects")]
    public IReadOnlyList<Project>? Projects { get; set; }

}
