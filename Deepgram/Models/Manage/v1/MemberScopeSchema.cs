namespace Deepgram.Models.Manage.v1;
public class MemberScopeSchema(string scope)
{
    /// <summary>
    /// Scope to add for member
    /// </summary>
    [JsonPropertyName("scope")]
    public string? Scope { get; set; } = scope;
}
