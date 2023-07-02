namespace Deepgram.Models;

public class ProjectList
{
    /// <summary>
    /// List of Deepgram projects
    /// </summary>
    [JsonProperty("projects")]
    public Project[] Projects { get; set; }
}
