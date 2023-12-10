namespace Deepgram.Models;
public class UpdateProjectMemberScopeSchema
{
    /// <summary>
    /// Scope to add for member
    /// </summary>
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}
