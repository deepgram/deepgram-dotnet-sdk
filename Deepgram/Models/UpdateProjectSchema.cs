namespace Deepgram.Models;
public class UpdateProjectSchema
{
    /// <summary>
    /// Name of Project
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Company who project belongs to 
    /// </summary>
    [JsonPropertyName("company")]
    public string? Company { get; set; }
}

