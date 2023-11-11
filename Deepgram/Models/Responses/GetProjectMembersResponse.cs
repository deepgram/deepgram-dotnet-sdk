namespace Deepgram.Models.Responses;

public class GetProjectMembersResponse
{
    [JsonPropertyName("members")]
    public Member[]? Members { get; set; }
}

