namespace Deepgram.Models;
public class UpdateProjectMemberScopeSchema(string scope)
{
    /// <summary>
    /// Scope to add for member
    /// </summary>
    [JsonPropertyName("scope")]
    public string Scope { get; set; } = scope;
}
