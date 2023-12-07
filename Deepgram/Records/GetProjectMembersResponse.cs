namespace Deepgram.Records;

public record GetProjectMembersResponse
{
    [JsonPropertyName("members")]
    public IReadOnlyList<Member>? Members { get; set; }
}

