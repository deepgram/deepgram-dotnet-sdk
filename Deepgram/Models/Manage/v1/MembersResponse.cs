namespace Deepgram.Models.Manage.v1;

public record MembersResponse
{
    [JsonPropertyName("members")]
    public IReadOnlyList<Member>? Members { get; set; }
}

