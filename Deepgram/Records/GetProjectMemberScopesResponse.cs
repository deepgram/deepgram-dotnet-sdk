namespace Deepgram.Records;

public record GetProjectMemberScopesResponse
{
    /// <summary>
    /// Project scopes associated with member
    /// </summary>
    [JsonPropertyName("scopes")]
    public IReadOnlyList<string>? Scopes { get; set; }
}
