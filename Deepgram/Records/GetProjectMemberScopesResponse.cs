namespace Deepgram.Records;

public record GetProjectMemberScopesResponse
{
    [JsonPropertyName("scopes")]
    public IReadOnlyList<string>? Scopes { get; set; }
}
