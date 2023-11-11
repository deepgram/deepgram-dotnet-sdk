namespace Deepgram.Models.Responses;

public class GetProjectMemberScopesResponse
{
    [JsonPropertyName("scopes")]
    public string[]? Scopes { get; set; }
}
