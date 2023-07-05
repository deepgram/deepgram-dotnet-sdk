namespace Deepgram.Models;

public class ProjectList
{
    /// <summary>
    /// List of Deepgram projects
    /// </summary>
    [JsonPropertyName("projects")]
    public Project[] Projects { get; set; }
}
