namespace Deepgram.Models.Manage.v1;

public record GetProjectMembersResponse
{
    [JsonPropertyName("members")]
    public IReadOnlyList<Member>? Members { get; set; }
}

