namespace Deepgram.Models.Responses;
public class GetProjectsResponse
{
    [JsonPropertyName("projects")]
    public Project[]? Projects { get; set; }

}
