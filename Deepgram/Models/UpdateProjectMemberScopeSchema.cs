namespace Deepgram.Models;
public class UpdateProjectMemberScopeSchema
{
    public UpdateProjectMemberScopeSchema(string scope)
    {
        Scope = scope;
    }
    /// <summary>
    /// Scope to add for member
    /// </summary>
    [JsonPropertyName("scope")]
    public string Scope { get; set; }
}
